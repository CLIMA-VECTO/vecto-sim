using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationData
	{
		private static DeclarationData _instance;
		private Segments _segments;
		private Rims _rims;
		private Wheels _wheels;
		private PT1 _pt1;
		private ElectricSystem _electricSystem;
		private Fan _fan;
		private HeatingVentilationAirConditioning _heatingVentilationAirConditioning;
		private PneumaticSystem _pneumaticSystem;
		private SteeringPump _steeringPump;
		private WHTCCorrection _whtcCorrection;
		private AirDrag _airDrag;

		public static Wheels Wheels
		{
			get { return Instance()._wheels ?? (Instance()._wheels = new Wheels()); }
		}

		public static Rims Rims
		{
			get { return Instance()._rims ?? (Instance()._rims = new Rims()); }
		}

		public static Segments Segments
		{
			get { return Instance()._segments ?? (Instance()._segments = new Segments()); }
		}

		public static PT1 PT1
		{
			get { return Instance()._pt1 ?? (Instance()._pt1 = new PT1()); }
		}

		public static ElectricSystem ElectricSystem
		{
			get { return Instance()._electricSystem ?? (Instance()._electricSystem = new ElectricSystem()); }
		}

		public static Meter DynamicTyreRadius(string wheels, string rims)
		{
			var wheelsEntry = Wheels.Lookup(wheels);
			var rimsEntry = Rims.Lookup(rims);

			var correction = wheelsEntry.SizeClass != "a" ? rimsEntry.F_a : rimsEntry.F_b;

			return wheelsEntry.DynamicTyreRadius * correction / (2 * Math.PI);
		}

		public static Fan Fan
		{
			get { return Instance()._fan ?? (Instance()._fan = new Fan()); }
		}

		public static HeatingVentilationAirConditioning HeatingVentilationAirConditioning
		{
			get
			{
				return Instance()._heatingVentilationAirConditioning ??
						(Instance()._heatingVentilationAirConditioning = new HeatingVentilationAirConditioning());
			}
		}

		public static PneumaticSystem PneumaticSystem
		{
			get { return Instance()._pneumaticSystem ?? (Instance()._pneumaticSystem = new PneumaticSystem()); }
		}

		public static SteeringPump SteeringPump
		{
			get { return Instance()._steeringPump ?? (Instance()._steeringPump = new SteeringPump()); }
		}

		public static WHTCCorrection WHTCCorrection
		{
			get { return Instance()._whtcCorrection ?? (Instance()._whtcCorrection = new WHTCCorrection()); }
		}

		public static AirDrag AirDrag
		{
			get { return Instance()._airDrag ?? (Instance()._airDrag = new AirDrag()); }
		}

		public static int PoweredAxle()
		{
			return 1;
		}


		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}

		//			Public Const SSspeed As Single = 5
		//Public Const SStime As Single = 5
		//Public Const SSdelay As Single = 5
		//Public Const LACa As Single = -0.5
		//Public Const LACvmin As Single = 50
		//Public Const Overspeed As Single = 5
		//Public Const Underspeed As Single = 5
		//Public Const ECvmin As Single = 50

		//Public Const AirDensity As Single = 1.188
		//Public Const FuelDens As Single = 0.832
		//Public Const CO2perFC As Single = 3.16

		//Public Const AuxESeff As Single = 0.7

		public static class Trailer
		{
			public const double RollResistanceCoefficient = 0.00555;
			public const double TyreTestLoad = 37500;
			public const bool TwinTyres = false;
			public const string WheelsType = "385/65 R 22.5";
		}

		public static class Engine
		{
			public static readonly KilogramSquareMeter ClutchInertia = 1.3.SI<KilogramSquareMeter>();
			public static readonly KilogramSquareMeter EngineBaseInertia = 0.41.SI<KilogramSquareMeter>();
			public static readonly SI EngineDisplacementInertia = (0.27 * 1000).SI().Kilo.Gramm.Per.Meter; // [kg/m]

			public static KilogramSquareMeter EngineInertia(SI displacement)
			{
				// VB Code:    Return 1.3 + 0.41 + 0.27 * (Displ / 1000)
				return (ClutchInertia + EngineBaseInertia + EngineDisplacementInertia * displacement).Cast<KilogramSquareMeter>();
			}
		}

		public static class Gearbox
		{
			public const double TorqueReserve = 20;
			public const double TorqueReserveStart = 20;
			public const double StartSpeed = 2;
			public const double StartAcceleration = 0.6;
			public const double Inertia = 0;

			public const double MinTimeBetweenGearshifts = 2;

			public static Second TractionInterruption(GearboxData.GearboxType type)
			{
				switch (type) {
					case GearboxData.GearboxType.MT:
						return 2.SI<Second>();
					case GearboxData.GearboxType.AMT:
						return 1.SI<Second>();
					case GearboxData.GearboxType.AT:
						return 0.8.SI<Second>();
				}
				return 0.SI<Second>();
			}

			public static bool EarlyShiftGears(GearboxData.GearboxType type)
			{
				switch (type) {
					case GearboxData.GearboxType.MT:
						return false;
					case GearboxData.GearboxType.AMT:
						return true;
					case GearboxData.GearboxType.AT:
						return false;
				}
				return false;
			}

			public static bool SkipGears(GearboxData.GearboxType type)
			{
				switch (type) {
					case GearboxData.GearboxType.MT:
						return true;
					case GearboxData.GearboxType.AMT:
						return true;
					case GearboxData.GearboxType.AT:
						return false;
				}
				return false;
			}

			internal static ShiftPolygon ComputeShiftPolygon(CombustionEngineData engine, uint gear)
			{
				var fullLoadCurve = engine.GetFullLoadCurve(gear);
				var idleSpeed = engine.IdleSpeed;

				var maxTorque = fullLoadCurve.MaxLoadTorque;

				var entriesDown = new List<ShiftPolygon.ShiftPolygonEntry>();
				var entriesUp = new List<ShiftPolygon.ShiftPolygonEntry>();

				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry() { AngularSpeed = idleSpeed, Torque = 0.SI<NewtonMeter>() });

				var tq1 = maxTorque * idleSpeed / (fullLoadCurve.PreferredSpeed + fullLoadCurve.LoSpeed - idleSpeed);
				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry() { AngularSpeed = idleSpeed, Torque = tq1 });

				var speed1 = (fullLoadCurve.PreferredSpeed + fullLoadCurve.LoSpeed) / 2;
				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry() { AngularSpeed = speed1, Torque = maxTorque });


				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry() {
					AngularSpeed = fullLoadCurve.PreferredSpeed,
					Torque = 0.SI<NewtonMeter>()
				});

				tq1 = maxTorque * (fullLoadCurve.PreferredSpeed - idleSpeed) / (fullLoadCurve.N95hSpeed - idleSpeed);
				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry() { AngularSpeed = fullLoadCurve.PreferredSpeed, Torque = tq1 });

				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry() { AngularSpeed = fullLoadCurve.N95hSpeed, Torque = maxTorque });

				return new ShiftPolygon(entriesDown, entriesUp);
			}
		}
	}

	internal class AirDrag : LookupData<VehicleCategory, double>
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.VCDV.parameters.csv";

		public AirDrag()
		{
			ParseData(ReadCsvResource(ResourceId));
		}


		protected override void ParseData(DataTable table)
		{
			// todo: constant!!
			var vWind = 3.SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>();

			// todo: get from vehicle or move whole procedure to vehicle
			var cdA0Actual = 0;

			Data.Clear();
			foreach (DataRow row in table.Rows) {
				var cat = row.Field<string>("Parameters");
				var values = new { a1 = row.ParseDouble("a1"), a2 = row.ParseDouble("a2"), a3 = row.ParseDouble("a3") };

				var betas = new List<double>();
				var deltaCdAs = new List<double>();
				for (var beta = 0; beta <= 12; beta++) {
					betas.Add(beta);
					var deltaCdA = values.a1 * beta + values.a2 * beta * beta + values.a3 * beta * beta * beta;
					deltaCdAs.Add(deltaCdA);
				}

				var cdX = new List<double> { 0 };
				var cdY = new List<double> { 0 };

				for (var vVeh = 60; vVeh <= 100; vVeh += 5) {
					var cdASum = 0.0;
					for (var alpha = 0; alpha <= 180; alpha += 10) {
						var vWindX = vWind * Math.Cos(alpha * Math.PI / 180);
						var vWindY = vWind * Math.Sin(alpha * Math.PI / 180);
						var vAirX = vVeh + vWindX;
						var vAirY = vWindY;
						var vAir = VectoMath.Sqrt<MeterPerSecond>(vAirX * vAirX + vAirY * vAirY);
						var beta = Math.Atan((vAirY / vAirX).Double()) * 180 / Math.PI;

						var k = 1;
						if (betas.First() < beta) {
							k = 0;
							while (betas[k] < beta && k < betas.Count) {
								k++;
							}
						}

						var deltaCdA = VectoMath.Interpolate(betas[k - 1], betas[k], deltaCdAs[k - 1], deltaCdAs[k], beta);

						var cdA = cdA0Actual + deltaCdA;

						var share = 10 / 180;
						if (vVeh == 0 || vVeh == 180) {
							share /= 2;
						}
						cdASum += share * cdA * (vAir * vAir / (vVeh * vVeh)).Double();
					}
					cdX.Add(vVeh);
					cdY.Add(cdASum);
				}

				cdY[0] = cdY[1];
			}
		}
	}
}
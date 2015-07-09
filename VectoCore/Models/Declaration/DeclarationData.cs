using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
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
		public static Fan Fan
		{
			var wheelsEntry = Wheels.Lookup(wheels);
			var rimsEntry = Rims.Lookup(rims);

			var correction = wheelsEntry.SizeClass != 0 ? rimsEntry.F_a : rimsEntry.F_b;

			return wheelsEntry.TyreRadius * correction / (2 * Math.PI);
		}

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
}
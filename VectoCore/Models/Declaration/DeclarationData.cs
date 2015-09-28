using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
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
		private AirDrag _airDrag;
		private TorqueConverter _torqueConverter;

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
			var wheelsEntry = Wheels.Lookup(wheels.Replace(" ", ""));
			var rimsEntry = Rims.Lookup(rims);

			var correction = wheelsEntry.SizeClass != "a" ? rimsEntry.F_b : rimsEntry.F_a;

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

		public static TorqueConverter TorqueConverter
		{
			get { return Instance()._torqueConverter ?? (Instance()._torqueConverter = new TorqueConverter()); }
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

		public static class Driver
		{
			public static class LookAhead
			{
				public const bool Enabled = true;

				public static readonly MeterPerSquareSecond Deceleration = -0.5.SI<MeterPerSquareSecond>();

				public static readonly MeterPerSecond MinimumSpeed = 50.KMPHtoMeterPerSecond();
			}

			public static class OverSpeedEcoRoll
			{
				public static readonly IList<DriverData.DriverMode> AllowedModes = new List<DriverData.DriverMode> {
					DriverData.DriverMode.EcoRoll,
					DriverData.DriverMode.Overspeed
				};

				public static readonly MeterPerSecond MinSpeed = 50.KMPHtoMeterPerSecond();
				public static readonly MeterPerSecond OverSpeed = 5.KMPHtoMeterPerSecond();
				public static readonly MeterPerSecond UnderSpeed = 5.KMPHtoMeterPerSecond();
			}

			public static class StartStop
			{
				public static readonly MeterPerSecond MaxSpeed = 5.KMPHtoMeterPerSecond();
				public static readonly Second Delay = 5.SI<Second>();
				public static readonly Second MinTime = 5.SI<Second>();
			}
		}

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
			public const double TorqueReserve = 0.2;
			public const double TorqueReserveStart = 0.2;
			public const double StartSpeed = 2;
			public const double StartAcceleration = 0.6;
			public const double Inertia = 0;

			public const double MinTimeBetweenGearshifts = 2;


			internal static ShiftPolygon ComputeShiftPolygon(EngineFullLoadCurve fullLoadCurve, PerSecond engineIdleSpeed)
			{
				var maxTorque = fullLoadCurve.MaxLoadTorque;

				var entriesDown = new List<ShiftPolygon.ShiftPolygonEntry>();
				var entriesUp = new List<ShiftPolygon.ShiftPolygonEntry>();

				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry {
					AngularSpeed = engineIdleSpeed,
					Torque = 0.SI<NewtonMeter>()
				});

				var tq1 = maxTorque * engineIdleSpeed / (fullLoadCurve.PreferredSpeed + fullLoadCurve.LoSpeed - engineIdleSpeed);
				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry { AngularSpeed = engineIdleSpeed, Torque = tq1 });

				var speed1 = (fullLoadCurve.PreferredSpeed + fullLoadCurve.LoSpeed) / 2;
				entriesDown.Add(new ShiftPolygon.ShiftPolygonEntry { AngularSpeed = speed1, Torque = maxTorque });


				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry {
					AngularSpeed = fullLoadCurve.PreferredSpeed,
					Torque = 0.SI<NewtonMeter>()
				});

				tq1 = maxTorque * (fullLoadCurve.PreferredSpeed - engineIdleSpeed) / (fullLoadCurve.N95hSpeed - engineIdleSpeed);
				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry { AngularSpeed = fullLoadCurve.PreferredSpeed, Torque = tq1 });

				entriesUp.Add(new ShiftPolygon.ShiftPolygonEntry { AngularSpeed = fullLoadCurve.N95hSpeed, Torque = maxTorque });

				return new ShiftPolygon(entriesDown, entriesUp);
			}
		}

		public static IEnumerable<string> AuxiliaryIDs()
		{
			return new[] {
				Constants.Auxiliaries.IDs.Fan, Constants.Auxiliaries.IDs.SteeringPump,
				Constants.Auxiliaries.IDs.HeatingVentilationAirCondition, Constants.Auxiliaries.IDs.ElectricSystem,
				Constants.Auxiliaries.IDs.PneumaticSystem
			};
		}
	}
}
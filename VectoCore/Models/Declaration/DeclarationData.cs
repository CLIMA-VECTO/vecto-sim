using System;
using System.Linq;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationData
	{
		private static DeclarationData _instance;
		private readonly DeclarationSegments _segments;
		private readonly DeclarationRims _rims;
		private readonly DeclarationWheels _wheels;
		private readonly DeclarationPT1 _pt1;
		private readonly ElectricSystem _electricSystem;

		public static DeclarationWheels Wheels
		{
			get { return Instance()._wheels; }
		}

		public static DeclarationRims Rims
		{
			get { return Instance()._rims; }
		}

		public static DeclarationSegments Segments
		{
			get { return Instance()._segments; }
		}

		public static DeclarationPT1 PT1
		{
			get { return Instance()._pt1; }
		}

		public static ElectricSystem ElectricSystem
		{
			get { return Instance()._electricSystem; }
		}

		public static Meter DynamicTyreRadius(string wheels, string rims)
		{
			var wheelsEntry = Wheels.Lookup(wheels);
			var rimsEntry = Rims.Lookup(rims);

			var correction = wheelsEntry.SizeClass != 0 ? rimsEntry.F_a : rimsEntry.F_b;

			return wheelsEntry.TyreRadius * correction / (2 * Math.PI);
		}

		private DeclarationData()
		{
			_wheels = new DeclarationWheels();
			_rims = new DeclarationRims();
			_segments = new DeclarationSegments();
			_pt1 = new DeclarationPT1();
			_electricSystem = new ElectricSystem();
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
		}
	}
}
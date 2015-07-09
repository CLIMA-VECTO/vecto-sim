using System.Data;

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

		private static DeclarationData Instance()
		{
			return _instance ?? (_instance = new DeclarationData());
		}
	}

	public class WHTCCorrection : LookupData<MissionType, double, double, double, double>
	{
		public override double Lookup(MissionType key1, double key2, double key3, double key4)
		{
			throw new System.NotImplementedException();
		}

		protected override string ResourceId
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override void ParseData(DataTable table)
		{
			throw new System.NotImplementedException();
		}
	}
}
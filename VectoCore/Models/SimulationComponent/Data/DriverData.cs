using System;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class DriverData
	{
		public enum DriverMode
		{
			Off,
			Overspeed,
			EcoRoll,
		}


		public VectoRunData.StartStopData StartStop;
		public OverSpeedEcoRollData OverSpeedEcoRoll;
		public LACData LookAheadCoasting;
		public AccelerationCurveData AccelerationCurve;


		public class OverSpeedEcoRollData
		{
			public DriverMode Mode;
			public MeterPerSecond MinSpeed;
			public MeterPerSecond OverSpeed;
			public MeterPerSecond UnderSpeed;
		}

		public class LACData
		{
			public bool Enabled;
			public MeterPerSquareSecond Deceleration;
			public MeterPerSecond MinSpeed;
		}
	}
}
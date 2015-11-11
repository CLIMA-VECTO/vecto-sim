using System.Collections.Generic;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	[DataContract]
	public class VectoRunData : SimulationComponentData
	{
		public VehicleData VehicleData { get; internal set; }

		public CombustionEngineData EngineData { get; internal set; }

		public GearboxData GearboxData { get; internal set; }

		public DrivingCycleData Cycle { get; internal set; }

		public IEnumerable<AuxData> Aux { get; internal set; }

		public string AccelerationLimitingFile { get; internal set; }

		public DriverData DriverData { get; internal set; }

		public bool IsEngineOnly { get; internal set; }

		//public StartStopData StartStop { get; internal set; }

		//public LACData LookAheadCoasting { get; internal set; }

		//public OverSpeedEcoRollData OverSpeedEcoRoll { get; internal set; }

		public string JobFileName { get; set; }

		public string BasePath { get; set; }
		public string ModFileSuffix { get; set; }
		public DeclarationReport Report { get; set; }
		public LoadingType Loading { get; set; }
		public Mission Mission { get; set; }


		public class AuxData
		{
			public string ID;
			public AuxiliaryType Type;
			public string Path;
			public string Technology;
			public string[] TechList;
			public Watt PowerDemand;
			public AuxiliaryDemandType DemandType;
			public AuxiliaryData Data;
		}

		public class StartStopData
		{
			public bool Enabled;
			public MeterPerSecond MaxSpeed;
			public Second MinTime;
			public Second Delay;
		}
	}
}
using System.Collections.Generic;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
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


		public class AuxData
		{
			public string ID;
			public AuxiliaryType Type;
			public string Path;
			public string Technology;
			public string[] TechList;
			public Watt PowerDemand;
			public AuxiliaryDemandType DemandType;
			public MappingAuxiliaryData Data;
		}

		public class StartStopData
		{
			public bool Enabled;
			public double MaxSpeed;
			public double MinTime;
			public double Delay;
		}

		public class LACData
		{
			public bool Enabled;
			public double Dec;
			public double MinSpeed;
		}

		public class OverSpeedEcoRollData
		{
			public string Mode;
			public double MinSpeed;
			public double OverSpeed;
			public double UnderSpeed;
		}
	}
}
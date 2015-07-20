using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent;
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

		public IList<AuxData> Aux { get; internal set; }

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
			public string Type;
			public string Path;
			public string Technology;

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	[DataContract]
	public class VectoJobData : SimulationComponentData
	{
		public VehicleData VehicleData { get; internal set; }

		public CombustionEngineData EngineData { get; internal set; }

		public GearboxData GearboxData { get; internal set; }

		public IList<string> Cycles { get; internal set; }

		public IList<AuxData> Aux { get; internal set; }

		public string AccelerationLimitingFile { get; internal set; }

		public bool IsEngineOnly { get; internal set; }

		//public StartStopData StartStop { get; internal set; }

		//public LACData LookAheadCoasting { get; internal set; }

		//public OverSpeedEcoRollData OverSpeedEcoRoll { get; internal set; }

		public string JobFileName { get; set; }


		public class AuxData
		{
			public string ID;
			public string Type;
			public string Path;
			public string Technology;
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
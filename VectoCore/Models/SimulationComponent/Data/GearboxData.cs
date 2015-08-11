using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.FileIO;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	[DataContract]
	public class GearboxData : SimulationComponentData
	{
		public enum GearboxType
		{
			MT, // Manual Transmission
			AMT, // Automated Manual Transmission
			AT, // Automatic Transmission
			Custom
		}

		public string ModelName { get; internal set; }

		public GearData AxleGearData { get; internal set; }

		internal readonly Dictionary<uint, GearData> _gearData = new Dictionary<uint, GearData>();

		public int GearsCount()
		{
			return _gearData.Count;
		}

		public GearData this[uint i]
		{
			get { return _gearData[i]; }
		}


		public GearboxType Type { get; internal set; }

		/// <summary>
		///		kgm^2
		/// </summary>
		public KilogramSquareMeter Inertia { get; internal set; }

		/// <summary>
		///		[s]
		/// </summary>
		public Second TractionInterruption { get; internal set; }

		/// <summary>
		///		[%] (0-1)
		/// </summary>
		public double TorqueReserve { get; internal set; }

		/// <summary>
		///		used by gear-shift model
		/// </summary>
		public bool SkipGears { get; internal set; }

		/// <summary>
		///		[s]
		/// </summary>
		public Second ShiftTime { get; internal set; }

		public bool EarlyShiftUp { get; internal set; }

		/// <summary>
		/// [%] (0-1)
		/// </summary>
		public double StartTorqueReserve { get; internal set; }

		/// <summary>
		/// [m/s]
		/// </summary>
		public MeterPerSecond StartSpeed { get; internal set; }

		/// <summary>
		/// [m/s^2]
		/// </summary>
		public SI StartAcceleration { get; internal set; }

		public bool HasTorqueConverter { get; internal set; }
	}
}
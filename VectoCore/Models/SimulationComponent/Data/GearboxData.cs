using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	/// <summary>
	/// Class for Gearbox Data. Gears can be accessed via Gears-Dictionary and range from 1 upwards.
	/// </summary>
	/// <remarks>The Axle Gear has its own Property "AxleGearData" and is *not included* in the Gears-Dictionary.</remarks>
	[DataContract]
	public class GearboxData : SimulationComponentData
	{
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		public enum GearboxType
		{
			MT, // Manual Transmission
			AMT, // Automated Manual Transmission
			AT, // Automatic Transmission
			Custom
		}

		public string ModelName { get; internal set; }

		public GearData AxleGearData { get; internal set; }

		public readonly Dictionary<uint, GearData> Gears = new Dictionary<uint, GearData>();

		public GearboxType Type { get; internal set; }

		public KilogramSquareMeter Inertia { get; internal set; }

		public Second TractionInterruption { get; internal set; }

		/// <summary>
		///		[%] (0-1)
		/// </summary>
		public double TorqueReserve { get; internal set; }

		/// <summary>
		///		used by gear-shift model
		/// </summary>
		public bool SkipGears { get; internal set; }

		public Second ShiftTime { get; internal set; }

		public bool EarlyShiftUp { get; internal set; }

		/// <summary>
		/// [%] (0-1)
		/// </summary>
		public double StartTorqueReserve { get; internal set; }

		public MeterPerSecond StartSpeed { get; internal set; }

		public MeterPerSquareSecond StartAcceleration { get; internal set; }

		public bool HasTorqueConverter { get; internal set; }
	}
}
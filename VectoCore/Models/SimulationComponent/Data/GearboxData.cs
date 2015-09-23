using System.Collections.Generic;
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
		public string ModelName { get; internal set; }

		public GearData AxleGearData { get; internal set; }

		public Dictionary<uint, GearData> Gears = new Dictionary<uint, GearData>();

		public GearboxType Type { get; internal set; }

		public KilogramSquareMeter Inertia { get; internal set; }

		public Second TractionInterruption { get; internal set; }

		/// <summary>
		///	[%] (0-1) The torque reserve for shift strategy (early upshift, skipgears)
		/// </summary>
		public double TorqueReserve { get; internal set; }

		/// <summary>
		///	Indicates if gears can be skipped in Gear Shift Strategy.
		/// </summary>
		public bool SkipGears { get; internal set; }

		public Second ShiftTime { get; internal set; }

		public bool EarlyShiftUp { get; internal set; }

		/// <summary>
		/// [%] (0-1) The starting torque reserve for finding the starting gear after standstill.
		/// </summary>
		public double StartTorqueReserve { get; internal set; }

		public MeterPerSecond StartSpeed { get; internal set; }

		public MeterPerSquareSecond StartAcceleration { get; internal set; }

		public bool HasTorqueConverter { get; internal set; }
	}
}
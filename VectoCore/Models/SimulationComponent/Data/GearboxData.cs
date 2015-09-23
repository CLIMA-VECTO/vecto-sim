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
		/// <summary>
		/// The name of the gearbox model.
		/// </summary>
		public string ModelName { get; internal set; }

		/// <summary>
		/// The axle gear data.
		/// </summary>
		public GearData AxleGearData { get; internal set; }

		/// <summary>
		/// The gear data.
		/// </summary>
		public Dictionary<uint, GearData> Gears = new Dictionary<uint, GearData>();

		/// <summary>
		/// Gets the type.
		/// </summary>
		/// <value>
		/// The type.
		/// </value>
		public GearboxType Type { get; internal set; }

		/// <summary>
		/// Gets the inertia.
		/// </summary>
		/// <value>
		/// The inertia.
		/// </value>
		public KilogramSquareMeter Inertia { get; internal set; }

		/// <summary>
		/// Gets the traction interruption.
		/// </summary>
		/// <value>
		/// The traction interruption.
		/// </value>
		public Second TractionInterruption { get; internal set; }

		/// <summary>
		///	[%] (0-1) The torque reserve for shift strategy (early upshift, skipgears)
		/// </summary>
		public double TorqueReserve { get; internal set; }

		/// <summary>
		///	Indicates if gears can be skipped in Gear Shift Strategy.
		/// </summary>
		public bool SkipGears { get; internal set; }

		/// <summary>
		/// Gets the minimum time between shifts.
		/// </summary>
		public Second ShiftTime { get; internal set; }

		/// <summary>
		/// True if the gearbox should do early up shifts.
		/// </summary>
		public bool EarlyShiftUp { get; internal set; }

		/// <summary>
		/// [%] (0-1) The starting torque reserve for finding the starting gear after standstill.
		/// </summary>
		public double StartTorqueReserve { get; internal set; }

		/// <summary>
		/// Gets the start speed.
		/// </summary>
		/// <value>
		/// The start speed.
		/// </value>
		public MeterPerSecond StartSpeed { get; internal set; }

		/// <summary>
		/// Gets the start acceleration.
		/// </summary>
		/// <value>
		/// The start acceleration.
		/// </value>
		public MeterPerSquareSecond StartAcceleration { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether this instance has torque converter.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has torque converter; otherwise, <c>false</c>.
		/// </value>
		public bool HasTorqueConverter { get; internal set; }
	}
}
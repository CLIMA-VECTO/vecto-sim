using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Common.Logging;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class DrivingCycleData : SimulationComponentData
	{
		public enum CycleType
		{
			EngineOnly,
			TimeBased,
			DistanceBased
		}

		internal DrivingCycleData() {}

		public List<DrivingCycleEntry> Entries { get; internal set; }

		public string Name { get; internal set; }


		public class DrivingCycleEntry
		{
			/// <summary>
			///     [m]	Travelled distance used for distance-based cycles. If "t"
			///     is also defined this column will be ignored.
			/// </summary>
			public Meter Distance { get; set; }

			/// <summary>
			///     [s]	Used for time-based cycles. If neither this nor the distance
			///     "s" is defined the data will be interpreted as 1Hz.
			/// </summary>
			public Second Time { get; set; }

			/// <summary>
			///     [m/s]	Required except for Engine Only Mode calculations.
			/// </summary>
			public MeterPerSecond VehicleTargetSpeed { get; set; }

			/// <summary>
			///     [rad]	Optional.
			/// </summary>
			public Radian RoadGradient { get; set; }

			/// <summary>
			///    [%]  Optional.
			/// </summary>
			public double RoadGradientPercent { get; set; }

			/// <summary>
			///		[m] relative altitude of the driving cycle over distance
			/// </summary>
			public Meter Altitude { get; set; }

			/// <summary>
			///     [s]	Required for distance-based cycles. Not used in time based
			///     cycles. "stop" defines the time the vehicle spends in stop phases.
			/// </summary>
			public Second StoppingTime { get; set; }

			/// <summary>
			///     [W]	Supply Power input for each auxiliary defined in the
			///     .vecto file where xxx matches the ID of the corresponding
			///     Auxiliary. ID's are not case sensitive and must not contain
			///     space or special characters.
			/// </summary>
			public Dictionary<string, Watt> AuxiliarySupplyPower { get; set; }

			/// <summary>
			///     [rad/s]	If "n" is defined VECTO uses that instead of the
			///     calculated engine speed value.
			/// </summary>
			public PerSecond EngineSpeed { get; set; }

			/// <summary>
			///     [-]	Gear input. Overwrites the gear shift model.
			/// </summary>
			public double Gear { get; set; }

			/// <summary>
			///     [W]	This power input will be directly added to the engine
			///     power in addition to possible other auxiliaries. Also used in
			///     Engine Only Mode.
			/// </summary>
			public Watt AdditionalAuxPowerDemand { get; set; }

			/// <summary>
			///     [m/s] Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public MeterPerSecond AirSpeedRelativeToVehicle { get; set; }

			/// <summary>
			///     [°] Only required if Cross Wind Correction is set to Vair and Beta Input.
			/// </summary>
			public double WindYawAngle { get; set; }

			/// <summary>
			///     [Nm] Effective engine torque at clutch. Only required in
			///     Engine Only Mode. Alternatively power "Pe" can be defined.
			///     Use "DRAG" to define motoring operation.
			/// </summary>
			public NewtonMeter EngineTorque { get; set; }

			public bool Drag { get; set; }
		}
	}
}
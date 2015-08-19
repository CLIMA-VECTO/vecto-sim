using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public abstract class AbstractResponse : IResponse
	{
		public Second SimulationInterval { get; set; }

		public abstract ResponseType ResponseType { get; }

		public Watt EnginePowerRequest { get; set; }

		public Watt ClutchPowerRequest { get; set; }

		public Watt GearboxPowerRequest { get; set; }

		public Watt AxlegearPowerRequest { get; set; }

		public Watt WheelsPowerRequest { get; set; }

		public Watt VehiclePowerRequest { get; set; }
	}

	/// <summary>
	/// Response when the Cycle is finished.
	/// </summary>
	public class ResponseCycleFinished : AbstractResponse
	{
		public override ResponseType ResponseType
		{
			get { return ResponseType.CycleFinished; }
		}
	}

	/// <summary>
	/// Response when a request was successful.
	/// </summary>
	public class ResponseSuccess : AbstractResponse
	{
		public override ResponseType ResponseType
		{
			get { return ResponseType.Success; }
		}
	}

	/// <summary>
	/// Response when the request resulted in an engine overload. 
	/// </summary>
	public class ResponseFailOverload : AbstractResponse
	{
		public Watt Delta { get; set; }
		public double Gradient { get; set; }

		public override ResponseType ResponseType
		{
			get { return ResponseType.FailOverload; }
		}
	}

	/// <summary>
	/// Response when the request should have another time interval.
	/// </summary>
	public class ResponseFailTimeInterval : AbstractResponse
	{
		public Second DeltaT { get; set; }

		public override ResponseType ResponseType
		{
			get { return ResponseType.FailTimeInterval; }
		}
	}

	public class ResponseDrivingCycleDistanceExceeded : AbstractResponse
	{
		public Meter MaxDistance { get; set; }

		public override ResponseType ResponseType
		{
			get { return ResponseType.DrivingCycleDistanceExceeded; }
		}
	}

	internal class ResponseDryRun : AbstractResponse
	{
		public Watt EngineDeltaFullLoad { get; set; }
		public Watt EngineDeltaDragLoad { get; set; }

		public override ResponseType ResponseType
		{
			get { return ResponseType.DryRun; }
		}
	}

	internal class ResponseGearShift : AbstractResponse
	{
		public override ResponseType ResponseType
		{
			get { return ResponseType.GearShift; }
		}
	}
}
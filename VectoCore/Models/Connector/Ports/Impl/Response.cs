using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public abstract class AbstractResponse : IResponse
	{
		public Second SimulationInterval { get; set; }

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
	public class ResponseCycleFinished : AbstractResponse {}

	/// <summary>
	/// Response when a request was successful.
	/// </summary>
	public class ResponseSuccess : AbstractResponse {}

	/// <summary>
	/// Response when the request resulted in an engine overload. 
	/// </summary>
	public class ResponseEngineOverload : AbstractResponse
	{
		public Watt Delta { get; set; }
		public double Gradient { get; set; }
	}

	/// <summary>
	/// Response when the request resulted in an engine overload. 
	/// </summary>
	public class ResponseGearboxOverload : AbstractResponse
	{
		public Watt Delta { get; set; }
		public double Gradient { get; set; }
	}


	/// <summary>
	/// Response when the request should have another time interval.
	/// </summary>
	public class ResponseFailTimeInterval : AbstractResponse
	{
		public Second DeltaT { get; set; }
	}

	public class ResponseDrivingCycleDistanceExceeded : AbstractResponse
	{
		public Meter MaxDistance { get; set; }
	}

	internal class ResponseDryRun : AbstractResponse
	{
		public Watt EngineDeltaFullLoad { get; set; }
		public Watt EngineDeltaDragLoad { get; set; }
		public Watt GearboxDeltaFullLoad { get; set; }
	}

	internal class ResponseGearShift : AbstractResponse {}
}
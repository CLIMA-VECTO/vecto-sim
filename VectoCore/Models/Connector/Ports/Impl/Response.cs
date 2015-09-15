using System;
using System.Linq;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public abstract class AbstractResponse : IResponse
	{
		public Second SimulationInterval { get; set; }

		public Meter SimulationDistance { get; set; }

		public MeterPerSquareSecond Acceleration { get; set; }

		public Watt EnginePowerRequest { get; set; }

		public Watt ClutchPowerRequest { get; set; }

		public Watt GearboxPowerRequest { get; set; }

		public Watt AxlegearPowerRequest { get; set; }

		public Watt WheelsPowerRequest { get; set; }

		public Watt VehiclePowerRequest { get; set; }

		public Watt BrakePower { get; set; }

		public Object Source { get; set; }

		public override string ToString()
		{
			var t = GetType();
			return string.Format("{0}{{{1}}}", t.Name,
				", ".Join(t.GetProperties().Select(p => string.Format("{0}: {1}", p.Name, p.GetValue(this)))));
		}
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
	/// Response when the request resulted in an engine or gearbox overload. 
	/// </summary>
	public class ResponseOverload : AbstractResponse
	{
		public Watt Delta { get; set; }
		public double Gradient { get; set; }
	}

	/// <summary>
	/// Response when the request resulted in an engine under-load. 
	/// </summary>
	public class ResponseUnderload : ResponseOverload {}


	public class ResponseSpeedLimitExceeded : AbstractResponse {}

	/// <summary>
	/// Response when the request should have another time interval.
	/// </summary>
	public class ResponseFailTimeInterval : AbstractResponse
	{
		public Second DeltaT { get; set; }
	}

	public class ResponseDrivingCycleDistanceExceeded : AbstractResponse
	{
		public ResponseDrivingCycleDistanceExceeded() {}
		public Meter MaxDistance { get; set; }
	}

	internal class ResponseDryRun : AbstractResponse
	{
		public Watt DeltaFullLoad { get; set; }
		public Watt DeltaDragLoad { get; set; }
	}

	internal class ResponseGearShift : AbstractResponse {}
}
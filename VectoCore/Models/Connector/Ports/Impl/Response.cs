using System;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public abstract class AbstractResponse : IResponse
	{
		public Second SimulationInterval { get; set; }

		public abstract ResponseType ResponseType { get; }
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
		public double Delta { get; set; }
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
		public double DeltaFullLoad { get; set; }
		public double DeltaDragLoad { get; set; }

		public override ResponseType ResponseType
		{
			get { return ResponseType.DryRun; }
		}
	}
}
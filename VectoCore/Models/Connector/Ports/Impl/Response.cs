using System;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	/// <summary>
	/// Response when the Cycle is finished.
	/// </summary>
	public class ResponseCycleFinished : IResponse {}

	/// <summary>
	/// Response when a request was successful.
	/// </summary>
	public class ResponseSuccess : IResponse {}

	/// <summary>
	/// Response when the request resulted in an engine overload. 
	/// </summary>
	public class ResponseFailOverload : IResponse
	{
		public double Delta { get; set; }
		public double Gradient { get; set; }
	}

	/// <summary>
	/// Response when the request should have another time interval.
	/// </summary>
	public class ResponseFailTimeInterval : IResponse
	{
		public TimeSpan DeltaT { get; set; }
	}
}
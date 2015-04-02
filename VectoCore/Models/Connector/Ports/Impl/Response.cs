using System;

namespace TUGraz.VectoCore.Models.Connector.Ports.Impl
{
	public class ResponseCycleFinished : IResponse {}

	public class ResponseSuccess : IResponse {}

	public class ResponseFailOverload : IResponse {}

	public class ResponseFailTimeInterval : IResponse
	{
		public ResponseFailTimeInterval(TimeSpan dt)
		{
			DeltaT = dt;
		}
		public TimeSpan DeltaT { get; set; }
	}

}
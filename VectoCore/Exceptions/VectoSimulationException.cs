using System;
using JetBrains.Annotations;
using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Exceptions
{
	public class VectoSimulationException : VectoException
	{
		public VectoSimulationException(string msg) : base(msg) {}
		public VectoSimulationException(string msg, Exception inner) : base(msg, inner) {}

		[StringFormatMethod("message")]
		public VectoSimulationException(string message, params object[] args) : base(message, args) {}
	}

	public class UnexpectedResponseException : VectoSimulationException
	{
		public IResponse Response;

		public UnexpectedResponseException(string message, IResponse resp) : base(message + " {0}", resp)
		{
			Response = resp;
		}
	}
}
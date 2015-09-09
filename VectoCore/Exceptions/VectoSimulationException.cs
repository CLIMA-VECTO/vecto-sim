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

		[StringFormatMethod("message")]
		public UnexpectedResponseException(string message, IResponse resp, params object[] args)
			: base(message + Environment.NewLine + resp.ToString().Replace("{", "{{").Replace("}", "}}"), args)
		{
			Response = resp;
		}
	}
}
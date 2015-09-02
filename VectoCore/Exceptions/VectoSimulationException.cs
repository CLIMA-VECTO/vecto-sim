using System;
using JetBrains.Annotations;

namespace TUGraz.VectoCore.Exceptions
{
	public class VectoSimulationException : VectoException
	{
		public VectoSimulationException(string msg) : base(msg) {}
		public VectoSimulationException(string msg, Exception inner) : base(msg, inner) {}

		[StringFormatMethod("message")]
		public VectoSimulationException(string message, params object[] args) : base(message, args) {}
	}
}
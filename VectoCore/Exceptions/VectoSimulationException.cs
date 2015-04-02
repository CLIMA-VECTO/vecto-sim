using System;

namespace TUGraz.VectoCore.Exceptions
{
	public class VectoSimulationException : VectoException
	{
		public VectoSimulationException(string msg) : base(msg) {}
		public VectoSimulationException(string msg, Exception inner) : base(msg, inner) {}
	}
}
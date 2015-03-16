using System;

namespace TUGraz.VectoCore.Exceptions
{
	class VectoSimulationException : Exception
	{
		public VectoSimulationException(string msg) : base(msg) { }

		public VectoSimulationException(string msg, Exception inner) : base(msg, inner) { }
	}

}

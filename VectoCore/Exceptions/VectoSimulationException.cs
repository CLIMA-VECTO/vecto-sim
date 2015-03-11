using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Exceptions
{
	class VectoSimulationException : Exception
	{
		public VectoSimulationException(string msg) : base(msg) { }

		public VectoSimulationException(string msg, Exception inner) : base(msg, inner) { }
	}

}

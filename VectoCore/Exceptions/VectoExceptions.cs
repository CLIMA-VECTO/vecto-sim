using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUGraz.VectoCore.Exceptions
{
	

	abstract class  FileIOException : Exception
	{
		protected FileIOException(string message) : base(message)
		{

		}

		protected FileIOException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	class UnsupportedFileVersion : FileIOException
	{
		public UnsupportedFileVersion(string message) : base(message) { }
		public UnsupportedFileVersion(string message, Exception inner) : base(message, inner) { }
	}
}

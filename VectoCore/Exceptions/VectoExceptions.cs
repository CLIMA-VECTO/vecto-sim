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

	class InvalidFileFormatException : FileIOException
	{
		public InvalidFileFormatException(string message) : base(message) { }
		public InvalidFileFormatException(string message, Exception inner) : base(message) { }
	}

	class UnsupportedFileVersionException : FileIOException
	{
		public UnsupportedFileVersionException(string message) : base(message) { }
		public UnsupportedFileVersionException(string message, Exception inner) : base(message, inner) { }
	}
}

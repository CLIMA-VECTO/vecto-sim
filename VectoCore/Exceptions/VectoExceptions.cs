using System;
using System.IO;

namespace TUGraz.VectoCore.Exceptions
{
	public class VectoException : Exception
	{
		public VectoException(string message) : base(message) {}
		public VectoException(string message, Exception innerException) : base(message, innerException) {}
	}

	public abstract class FileIOException : VectoException
	{
		protected FileIOException(string message) : base(message) {}
		protected FileIOException(string message, Exception inner) : base(message, inner) {}
	}


	public class InvalidFileFormatException : FileIOException
	{
		public InvalidFileFormatException(string message) : base(message) {}
		public InvalidFileFormatException(string message, Exception inner) : base(message) {}
	}


	/// <summary>
	///     Exception which gets thrown when the version of a file is not supported.
	/// </summary>
	public class UnsupportedFileVersionException : FileIOException
	{
		public UnsupportedFileVersionException(string message) : base(message) {}
		public UnsupportedFileVersionException(string message, Exception inner) : base(message, inner) {}

		public UnsupportedFileVersionException(string filename, int version, Exception inner = null)
			: base(string.Format("Unsupported Version of {0} file. Got Version {1}",
				Path.GetExtension(filename), version), inner) {}
	}

	/// <summary>
	///     Exception which gets thrown when an error occurred during read of a vecto csv-file.
	/// </summary>
	public class CSVReadException : FileIOException
	{
		public CSVReadException(string message) : base(message) {}
		public CSVReadException(string message, Exception inner) : base(message, inner) {}
	}
}
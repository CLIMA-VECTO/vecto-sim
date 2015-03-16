using System;

namespace TUGraz.VectoCore.Exceptions
{
    class VectoException : Exception
    {
        public VectoException(string message) : base(message) { }
        public VectoException(string message, Exception innerException) : base(message, innerException) { }
    }

    abstract class FileIOException : VectoException
    {
        protected FileIOException(string message) : base(message) { }
        protected FileIOException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception which gets thrown when the version of a file is not supported.
    /// </summary>
    class UnsupportedFileVersion : FileIOException
    {
        public UnsupportedFileVersion(string message) : base(message) { }
        public UnsupportedFileVersion(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception which gets thrown when an error occurred during read of a vecto csv-file.
    /// </summary>
    class CSVReadException : FileIOException
    {
        public CSVReadException(string message) : base(message) { }
        public CSVReadException(string message, Exception inner) : base(message, inner) { }
    }
}

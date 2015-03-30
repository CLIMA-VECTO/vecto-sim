using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Exceptions
{
    [TestClass]
    public class ExceptionTests
    {
        [TestMethod]
        public void Test_VectoExceptions()
        {
            new CSVReadException("Test");
            new CSVReadException("Test", new Exception("Inner"));
            new InvalidFileFormatException("Test");
            new CSVReadException("Test", new Exception("Inner"));
            new UnsupportedFileVersionException("Test");
            new UnsupportedFileVersionException("Test", new Exception("Inner"));
            new InvalidFileFormatException("Test");
            new InvalidFileFormatException("Test", new Exception("Inner"));
            new VectoException("Test");
            new VectoException("Test", new Exception("Inner"));

            new VectoSimulationException("Test");
            new VectoSimulationException("Test", new Exception("Inner"));
        }
    }
}

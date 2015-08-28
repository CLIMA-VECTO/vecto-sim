using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;

namespace VectoConsole
{
	internal static class Program
	{
		private const string USAGE = @"Usage: vecto.exe [-h] [-v] FILE1.vecto [FILE2.vecto ...]";

		private const string HELP = @"
Commandline Interface for Vecto.

Synopsis:
    vecto.exe [-h] [-v] FILE1.vecto [FILE2.vecto ...]

Description:
    FILE1.vecto [FILE2.vecto ...]: A list of vecto-job files (with the extension: .vecto). At least one file must be given. Delimited by whitespace.
    -v: Activates verbose mode (trace and exceptions will be displayed)
    -h: Displays this help.

Examples:
    vecto.exe ""12t Delivery Truck.vecto"" 24tCoach.vecto 40t_Long_Haul_Truck.vecto
    vecto.exe 24tCoach.vecto 40t_Long_Haul_Truck.vecto
    vecto.exe -v 24tCoach.vecto
    vecto.exe -v jobs\40t_Long_Haul_Truck.vecto
	vecto.exe -h
";

		private static int Main(string[] args)
		{
			try {
				// on -h display help and terminate.
				if (args.Contains("-h")) {
					Console.Write(HELP);
					return 0;
				}
				args = args.Where(a => a != "-h").ToArray();

				// on -v: activate verbose console logger
				if (args.Contains("-v")) {
					var config = LogManager.Configuration;
					var target = config.FindTargetByName("ConsoleLogger");
					config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
					LogManager.Configuration = config;
					Trace.Listeners.Add(new ConsoleTraceListener(true));
				}
				args = args.Where(a => a != "-v").ToArray();

				// if no other arguments given: display usage and terminate
				if (!args.Any()) {
					Console.Write(USAGE);
					return 1;
				}

				// process the file list and start simulation
				var fileList = args.Where(a => a != "-v").ToList().ToList();

				var sumFileName = Path.GetFileNameWithoutExtension(fileList.First()) + Constants.FileExtensions.SumFile;
				var sumWriter = new SummaryFileWriter(sumFileName);
				var jobContainer = new JobContainer(sumWriter);

				foreach (var file in fileList.Where(f => Path.GetExtension(f) == Constants.FileExtensions.VectoJobFile)) {
					var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.EngineeringMode);
					runsFactory.DataReader.SetJobFile(file);
					jobContainer.AddRuns(runsFactory);
				}

				jobContainer.Execute();
			} catch (Exception e) {
				Console.Error.WriteLine(e.Message);
				Trace.TraceError(e.ToString());
				Environment.ExitCode = Environment.ExitCode != 0 ? Environment.ExitCode : 1;
			}
			return Environment.ExitCode;
		}
	}
}
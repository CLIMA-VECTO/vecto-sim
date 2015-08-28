﻿using System;
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
		private static int Main(string[] args)
		{
			try {
				if (!args.Any()) {
					Console.Write("Usage: vecto.exe FILE1.vecto [FILE2.vecto ...]");
					return 1;
				}

				if (args.Contains("-v")) {
					var config = LogManager.Configuration;
					var target = config.FindTargetByName("ConsoleLogger");
					config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
					LogManager.Configuration = config;
					Trace.Listeners.Add(new ConsoleTraceListener(true));
				}

				var fileList = args.Where(a => a != "-v").ToList().ToList();

				var sumFileName = Path.GetFileNameWithoutExtension(fileList.First()) + Constants.FileExtensions.SumFile;
				var sumWriter = new SummaryFileWriter(sumFileName);
				var jobContainer = new JobContainer(sumWriter);

				foreach (var file in fileList.Where(f => Path.GetExtension(f) == Constants.FileExtensions.VectoJobFile)) {
					var runsFactory = new SimulatorFactory(SimulatorFactory.FactoryMode.DeclarationMode);
					runsFactory.DataReader.SetJobFile(file);
					jobContainer.AddRuns(runsFactory);
				}

				jobContainer.Execute();
			} catch (Exception e) {
				Console.Error.WriteLine(e.Message);
				Trace.TraceError(e.ToString());
				Environment.ExitCode = Environment.ExitCode != 0 ? Environment.ExitCode : 1;
			} finally {
				Console.Read();
			}
			return Environment.ExitCode;
		}
	}
}
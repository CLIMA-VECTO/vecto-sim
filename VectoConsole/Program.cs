﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Threading;
using NLog;
using NLog.Config;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;

namespace VectoConsole
{
	internal static class Program
	{
		private static int NumLines;
		private static int ProgessCounter { get; set; }

		private const string USAGE = @"Usage: vecto.exe [-h] [-v] FILE1.vecto [FILE2.vecto ...]";

		private const string HELP = @"
Commandline Interface for Vecto.

Synopsis:
    vecto.exe [-h] [-v] FILE1.vecto [FILE2.vecto ...]

Description:
    FILE1.vecto [FILE2.vecto ...]: A list of vecto-job files (with the 
       extension: .vecto). At least one file must be given. Delimited by 
       whitespace.

    -t: output information about execution times
    -mod: write mod-data in addition to sum-data
    -eng: switch to engineering mode (implies -mod)
    -v: Shows verbose information (errors and warnings will be displayed)
	-vv: Shows more verbose information (infos will be displayed)
	-vvv: Shows debug messages (slow!)
	-vvvv: Shows all verbose information (everything, slow!)
    -V: show version information
    -h: Displays this help.

Examples:
    vecto.exe ""12t Delivery Truck.vecto"" 40t_Long_Haul_Truck.vecto
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
					ShowVersionInformation();
					Console.Write(HELP);
					return 0;
				}

				// on -v: activate verbose console logger
				var logLevel = LogLevel.Fatal;

				// Fatal > Error > Warn > Info > Debug > Trace
				var debugEnabled = false;

				if (args.Contains("-v")) {
					// display errors, warnings
					logLevel = LogLevel.Warn;
					debugEnabled = true;
				} else if (args.Contains("-vv")) {
					// also display info
					logLevel = LogLevel.Info;
					debugEnabled = true;
				} else if (args.Contains("-vvv")) {
					// display debug messages
					logLevel = LogLevel.Debug;
					debugEnabled = true;
				} else if (args.Contains("-vvvv")) {
					// display everything!
					logLevel = LogLevel.Trace;
					debugEnabled = true;
				}

				var config = LogManager.Configuration;
				//config.LoggingRules.Add(new LoggingRule("*", logLevel, config.FindTargetByName("ConsoleLogger")));
				config.LoggingRules.Add(new LoggingRule("*", logLevel, config.FindTargetByName("LogFile")));
				LogManager.Configuration = config;
				Trace.Listeners.Add(new ConsoleTraceListener(true));

				if (args.Contains("-V") || debugEnabled) {
					ShowVersionInformation();
				}

				args = args.Except(new[] { "-v", "-vv", "-vvv", "-vvvv", "-V" }).ToArray();

				// if no other arguments given: display usage and terminate
				if (!args.Any()) {
					Console.Write(USAGE);
					return 1;
				}

				var stopWatch = new Stopwatch();
				var timings = new Dictionary<string, double>();

				// process the file list and start simulation
				var fileList = args;

				var sumFileName = Path.GetFileNameWithoutExtension(fileList.First()) + Constants.FileExtensions.SumFile;
				var sumWriter = new SummaryFileWriter(sumFileName);
				var jobContainer = new JobContainer(sumWriter);

				var mode = SimulatorFactory.FactoryMode.DeclarationMode;
				if (args.Contains("-eng")) {
					mode = SimulatorFactory.FactoryMode.EngineeringMode;
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("Switching to Engineering Mode. Make sure the job-file is saved in engineering mode!");
					Console.ResetColor();
				}

				Console.WriteLine("Reading Job Files");
				stopWatch.Start();
				foreach (var file in fileList.Where(f => Path.GetExtension(f) == Constants.FileExtensions.VectoJobFile)) {
					var runsFactory = new SimulatorFactory(mode, file);
					if (args.Contains("-mod")) {
						runsFactory.WriteModalResults = true;
					}
					jobContainer.AddRuns(runsFactory);
				}
				stopWatch.Stop();
				timings.Add("Reading input files", stopWatch.Elapsed.TotalMilliseconds);
				stopWatch.Reset();

				Console.WriteLine("Starting simulation runs");
				if (debugEnabled) {
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("Debug-Output is enabled, executing simulation runs sequentially");
					Console.ResetColor();
				}
				Console.WriteLine();
				stopWatch.Start();
				jobContainer.Execute(!debugEnabled);

				Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) => {
					var isCtrlC = e.SpecialKey == ConsoleSpecialKey.ControlC;
					var isCtrlBreak = e.SpecialKey == ConsoleSpecialKey.ControlBreak;

					if (isCtrlC) {
						Console.WriteLine("Canceling simulation!");
					}
				};

				//var x = Console.CursorLeft;
				while (!jobContainer.AllCompleted) {
					PrintProgress(jobContainer.GetProgress());
					Thread.Sleep(250);
				}
				stopWatch.Stop();
				timings.Add("Simulation runs", stopWatch.Elapsed.TotalMilliseconds);

				PrintProgress(jobContainer.GetProgress(), args.Contains("-t"));
				if (args.Contains("-t")) {
					PrintTimings(timings);
				}
			} catch (Exception e) {
				Console.Error.WriteLine(e.Message);
				//Trace.TraceError(e.ToString());
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine("Please see log-file for further details (logs/log.txt)");
				Console.ResetColor();
				Environment.ExitCode = Environment.ExitCode != 0 ? Environment.ExitCode : 1;
			}
			return Environment.ExitCode;
		}

		private static void ShowVersionInformation()
		{
			var vectodll = AssemblyName.GetAssemblyName("VectoCore.dll");
			Console.WriteLine("VectoConsole: {0}", Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("VectoCore: {0}", vectodll.Version);
		}

		private static void PrintProgress(Dictionary<string, JobContainer.ProgressEntry> progessData, bool showTiming = true)
		{
			Console.SetCursorPosition(0, Console.CursorTop - NumLines);
			NumLines = 0;
			var sumProgress = 0.0;
			foreach (var progressEntry in progessData) {
				if (progressEntry.Value.Success) {
					Console.ForegroundColor = ConsoleColor.Green;
				} else if (progressEntry.Value.Error != null) {
					Console.ForegroundColor = ConsoleColor.Red;
				}
				var timingString = "";
				if (showTiming && progressEntry.Value.ExecTime > 0) {
					timingString = string.Format("{0,9:F2}s", progressEntry.Value.ExecTime / 1000.0);
				}
				Console.WriteLine("{0,-60} {1,8:P}{2}", progressEntry.Key, progressEntry.Value.Progress, timingString);
				Console.ResetColor();
				sumProgress += progressEntry.Value.Progress;
				NumLines++;
			}
			sumProgress /= NumLines;
			var spinner = "/-\\|"[ProgessCounter++ % 4];
			var bar = new string('#', (int)(sumProgress * 100.0 / 2));
			Console.WriteLine(string.Format("   {2}   [{1,-50}]  [{0,7:P}]", sumProgress, bar, spinner));
			NumLines++;
		}

		private static void PrintTimings(Dictionary<string, double> timings)
		{
			Console.WriteLine();
			Console.WriteLine("---- timing information ----");
			foreach (var timing in timings) {
				Console.WriteLine("{0,-20}: {1:F2}s", timing.Key, timing.Value / 1000);
			}
		}
	}
}
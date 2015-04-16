﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
	[TestClass]
	public class RetarderTest
	{
		private const string RetarderLossMapFile = @"TestData\Components\Retarder.vrlm";
		private const double Delta = 0.0001;

		[TestMethod]
		public void RetarderBasicTest()
		{
			var vehicle = new VehicleContainer();
			var retarderData = RetarderLossMap.ReadFromFile(RetarderLossMapFile);
			var retarder = new Retarder(vehicle, retarderData);

			var nextRequest = new MockTnOutPort();

			retarder.InShaft().Connect(nextRequest);
			var outPort = retarder.OutShaft();

			var absTime = TimeSpan.FromSeconds(0);
			var dt = TimeSpan.FromSeconds(0);

			// --------
			outPort.Request(absTime, dt, 0.SI<NewtonMeter>(), 10.RPMtoRad());

			Assert.AreEqual(10.RPMtoRad().Double(), (double) nextRequest.AngularVelocity, Delta);
			Assert.AreEqual(10.002, (double) nextRequest.Torque, Delta);

			// --------
			outPort.Request(absTime, dt, 100.SI<NewtonMeter>(), 1000.RPMtoRad());

			Assert.AreEqual(1000.RPMtoRad().Double(), (double) nextRequest.AngularVelocity, Delta);
			Assert.AreEqual(112, (double) nextRequest.Torque, Delta);

			// --------

			outPort.Request(absTime, dt, 50.SI<NewtonMeter>(), 1550.RPMtoRad());

			Assert.AreEqual(1550.RPMtoRad().Double(), (double) nextRequest.AngularVelocity, Delta);
			Assert.AreEqual(50 + 14.81, (double) nextRequest.Torque, Delta);
		}
	}
}
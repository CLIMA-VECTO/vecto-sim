using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.FileIO
{
	[TestClass]
	public class SimulationDataReaderTest
	{
		protected const string DeclarationJob = @"TestData\Jobs\12t Delivery Truck.vecto";
		protected const double Tolerance = 0.0001;

		[TestMethod]
		public void ReadDeclarationJobFile()
		{
			var reader = new DeclarationModeSimulationDataReader();
			reader.SetJobFile(DeclarationJob);

			var runData = reader.NextRun().First();

			Assert.AreEqual(false, runData.IsEngineOnly);

			Assert.AreEqual(Path.GetFileName(DeclarationJob), runData.JobFileName);
			Assert.AreEqual(5850, runData.VehicleData.CurbWeight.Value());
			Assert.AreEqual(1900, runData.VehicleData.CurbWeigthExtra.Value()); // taken from segmentation table
			Assert.AreEqual(11900, runData.VehicleData.GrossVehicleMassRating.Value());
			Assert.AreEqual(AxleConfiguration.AxleConfig_4x2, runData.VehicleData.AxleConfiguration);
			Assert.AreEqual(0.4069297458, runData.VehicleData.DynamicTyreRadius.Value(), Tolerance);

			Assert.AreEqual("2", runData.VehicleData.VehicleClass);
			Assert.AreEqual(2, runData.VehicleData.AxleData.Count);
			Assert.AreEqual(6, runData.VehicleData.AxleData[0].Inertia.Value(), Tolerance);

			Assert.AreEqual(true, runData.DriverData.LookAheadCoasting.Enabled);
			Assert.AreEqual(DeclarationData.Driver.LookAhead.MinimumSpeed.Value(),
				runData.DriverData.LookAheadCoasting.MinSpeed.Value(), Tolerance);
			Assert.AreEqual(DeclarationData.Driver.LookAhead.Deceleration.Value(),
				runData.DriverData.LookAheadCoasting.Deceleration.Value(), Tolerance);

			Assert.AreNotEqual(DriverData.DriverMode.Off, runData.DriverData.OverSpeedEcoRoll.Mode);
			Assert.AreEqual(DeclarationData.Driver.OverSpeedEcoRoll.MinSpeed.Value(),
				runData.DriverData.OverSpeedEcoRoll.MinSpeed.Value(), Tolerance);
			Assert.AreEqual(DeclarationData.Driver.OverSpeedEcoRoll.OverSpeed.Value(),
				runData.DriverData.OverSpeedEcoRoll.OverSpeed.Value(), Tolerance);
			Assert.AreEqual(DeclarationData.Driver.OverSpeedEcoRoll.UnderSpeed.Value(),
				runData.DriverData.OverSpeedEcoRoll.UnderSpeed.Value(), Tolerance);

			//Assert.AreEqual(false, runData.DriverData.StartStop.Enabled);
			Assert.AreEqual(DeclarationData.Driver.StartStop.Delay.Value(), runData.DriverData.StartStop.Delay.Value(), Tolerance);
			Assert.AreEqual(DeclarationData.Driver.StartStop.MaxSpeed.Value(), runData.DriverData.StartStop.MaxSpeed.Value(),
				Tolerance);
			Assert.AreEqual(DeclarationData.Driver.StartStop.MinTime.Value(), runData.DriverData.StartStop.MinTime.Value(),
				Tolerance);

			Assert.AreEqual(3.7890, runData.EngineData.Inertia.Value());

			var downshiftSpeeds = new double[] { 600, 600, 1310.6673 };
			var downshiftTorque = new double[] { 0, 266.85346, 899 };

			Assert.AreEqual(downshiftSpeeds.Length, runData.GearboxData[1].ShiftPolygon.Downshift.Count);
			for (var i = 0; i < downshiftSpeeds.Length; i++) {
				Assert.AreEqual(downshiftSpeeds[i].RPMtoRad().Value(),
					runData.GearboxData[1].ShiftPolygon.Downshift[i].AngularSpeed.Value(), Tolerance);
				Assert.AreEqual(downshiftTorque[i], runData.GearboxData[1].ShiftPolygon.Downshift[i].Torque.Value(), Tolerance);
			}

			var upshiftSpeed = new double[] { 1531.230044, 1531.230044, 2420.505793661 };
			var upshiftTorque = new double[] { 0, 459.8588, 899 };

			Assert.AreEqual(upshiftSpeed.Length, runData.GearboxData[1].ShiftPolygon.Downshift.Count);
			for (var i = 0; i < downshiftSpeeds.Length; i++) {
				Assert.AreEqual(upshiftSpeed[i].RPMtoRad().Value(),
					runData.GearboxData[1].ShiftPolygon.Upshift[i].AngularSpeed.Value(), Tolerance);
				Assert.AreEqual(upshiftTorque[i], runData.GearboxData[1].ShiftPolygon.Upshift[i].Torque.Value(), Tolerance);
			}
			//Assert.AreEqual();


			//Assert.AreEqual(, runData.DriverData.LookAheadCoasting.);
		}
	}
}
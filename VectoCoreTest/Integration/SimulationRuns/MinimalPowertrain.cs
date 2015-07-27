using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.FileIO.Reader;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;
using TUGraz.VectoCore.Utils;
using Wheels = TUGraz.VectoCore.Models.SimulationComponent.Impl.Wheels;

namespace TUGraz.VectoCore.Tests.Integration.SimulationRuns
{
	[TestClass]
	public class MinimalPowertrain
	{
		public const string CycleFile = @"TestData\Integration\MinimalPowerTrain\Coach_24t_xshort.vdri";
		public const string EngineFile = @"TestData\Integration\MinimalPowerTrain\24t Coach.veng";

		public const string AccelerationFile = @"TestData\Components\Coach.vacc";

		[TestMethod]
		public void TestWheelsAndEngine()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleDataReader.ReadFromFileDistanceBased(CycleFile);

			var vehicleData = new VehicleData() {
				AxleConfiguration = AxleConfiguration.AxleConfig_4x2,
				CrossSectionArea = 0.SI<SquareMeter>(),
				CrossWindCorrectionMode = CrossWindCorrectionMode.NoCorrection,
				CurbWeight = 1000.SI<Kilogram>(),
				CurbWeigthExtra = 0.SI<Kilogram>(),
				Loading = 0.SI<Kilogram>(),
				DynamicTyreRadius = 0.56.SI<Meter>(),
				Retarder = new RetarderData() { Type = RetarderData.RetarderType.None },
				AxleData = new List<Axle>(),
				SavedInDeclarationMode = false,
			};

			var driverData = new DriverData() {
				AccelerationCurve = AccelerationCurveData.ReadFromFile(AccelerationFile),
				LookAheadCoasting = new DriverData.LACData() {
					Enabled = false,
				},
				OverSpeedEcoRoll = new DriverData.OverSpeedEcoRollData() {
					Mode = DriverData.DriverMode.Off
				},
				StartStop = new VectoRunData.StartStopData() {
					Enabled = false,
				}
			};

			var modalWriter = new ModalDataWriter("Coach_MinimalPowertrain.vmod", false); //new TestModalDataWriter();
			var sumWriter = new TestSumWriter();
			var vehicleContainer = new VehicleContainer(modalWriter, sumWriter);

			var cycle = new DistanceBasedDrivingCycle(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new Driver(vehicleContainer, driverData));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new Clutch(vehicleContainer, engineData));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var gbx = new DummyGearbox(vehicleContainer);

			var cyclePort = cycle.OutPort();

			cyclePort.Initialize();

			gbx.CurrentGear = 0;

			var absTime = 0.SI<Second>();
			var response = cyclePort.Request(absTime, 1.SI<Meter>());

			Assert.IsInstanceOfType(response, typeof(ResponseSuccess));

			vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
			absTime += response.SimulationInterval;

			gbx.CurrentGear = 1;
			var ds = Constants.SimulationSettings.DriveOffDistance;
			while (vehicleContainer.Distance().Value() < 2000) {
				response = cyclePort.Request(absTime, ds);

				switch (response.ResponseType) {
					case ResponseType.DrivingCycleDistanceExceeded:
						var rsp = response as ResponseDrivingCycleDistanceExceeded;
						ds = rsp.MaxDistance;
						continue;
				}
				Assert.IsInstanceOfType(response, typeof(ResponseSuccess));


				vehicleContainer.CommitSimulationStep(absTime, response.SimulationInterval);
				absTime += response.SimulationInterval;

				ds = vehicleContainer.VehicleSpeed().IsEqual(0)
					? Constants.SimulationSettings.DriveOffDistance
					: (Constants.SimulationSettings.TargetTimeInterval * vehicleContainer.VehicleSpeed()).Cast<Meter>();

				modalWriter.Finish();
			}

			modalWriter.Finish();
			//var run = new DistanceRun(vehicleContainer);
			//run.Run();
		}


		// ========================

		protected virtual IDriver AddComponent(IDrivingCycle prev, IDriver next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IWheels AddComponent(IFvInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		protected virtual ITnOutProvider AddComponent(IWheels prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual void AddComponent(IPowerTrainComponent prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
		}
	}
}
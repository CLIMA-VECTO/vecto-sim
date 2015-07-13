using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.FileIO.Reader.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Declaration;
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
		public const string CycleFile = @"TestData\Cycles\Coach_24t_xshort.vdri";
		public const string EngineFile = @"TestData\Components\24t Coach.veng";

		[TestMethod]
		public void TestWheelsAndEngine()
		{
			var engineData = EngineeringModeSimulationDataReader.CreateEngineDataFromFile(EngineFile);
			var cycleData = DrivingCycleData.ReadFromFileDistanceBased(CycleFile);

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

			var vehicleContainer = new VehicleContainer();

			var cycle = new DistanceBasedSimulation(vehicleContainer, cycleData);

			dynamic tmp = AddComponent(cycle, new MockDriver(vehicleContainer));
			tmp = AddComponent(tmp, new Vehicle(vehicleContainer, vehicleData));
			tmp = AddComponent(tmp, new Wheels(vehicleContainer, vehicleData.DynamicTyreRadius));
			AddComponent(tmp, new CombustionEngine(vehicleContainer, engineData));

			var run = new DistanceRun(vehicleContainer);

			run.Run();
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


		protected virtual void AddComponent(IWheels prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
			//return next;
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
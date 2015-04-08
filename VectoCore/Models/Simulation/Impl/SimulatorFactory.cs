using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class SimulatorFactory
    {
        public static IVectoSimulator CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile,
            string resultFile)
        {
            Action<string> debug = LogManager.GetLogger<SimulatorFactory>().Debug;

            debug("Creating VehicleContainer.");
            var container = new VehicleContainer();

            debug("Creating cycle.");
            var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
            var cycle = new EngineOnlyDrivingCycle(container, cycleData);

            debug("Creating engine.");
            var engineData = CombustionEngineData.ReadFromFile(engineFile);
            var engine = new CombustionEngine(container, engineData);

            debug("Creating gearbox.");
            var gearBox = new EngineOnlyGearbox(container);

            debug("Creating auxiliary");
            var aux = new EngineOnlyAuxiliary(container, new AuxiliariesDemandAdapter(cycleData));

            debug("Connecting auxiliary with engine.");
            aux.Connect(engine.OutShaft());

            debug("Connecting gearbox with auxiliary.");
            gearBox.InShaft().Connect(aux.OutShaft());

            debug("Connecting cycle with gearbox.");
            cycle.InShaft().Connect(gearBox.OutShaft());

            var dataWriter = new ModalDataWriter(resultFile);

            debug("Creating Simulator.");
            //todo: load job file?
            var simulator = new VectoSimulator(container, cycle, dataWriter);

            return simulator;
        }
    }
}
using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class SimulatorFactory
    {
        /// <summary>
        /// Creates a time based engine only powertrain and simulation job for the given files.
        /// </summary>
        /// <param name="engineFile"></param>
        /// <param name="cycleFile"></param>
        /// <param name="resultFile"></param>
        /// <returns></returns>
        public static IVectoSimulator CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile,
            string resultFile)
        {
            Action<string> debug = LogManager.GetLogger<SimulatorFactory>().Debug;

            debug("Creating VehicleContainer.");
            var container = new VehicleContainer();

            debug("SimulationFactory creating cycle.");
            var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
            var cycle = new EngineOnlyDrivingCycle(container, cycleData);

            debug("SimulationFactory creating engine.");
            var engineData = CombustionEngineData.ReadFromFile(engineFile);
            var engine = new CombustionEngine(container, engineData);

            debug("Creating gearbox.");
            var gearBox = new EngineOnlyGearbox(container);

            debug("SimulationFactory creating auxiliary");
            var aux = new EngineOnlyAuxiliary(container, new AuxiliariesDemandAdapter(cycleData));

            debug("SimulationFactory connecting auxiliary with engine.");
            aux.InShaft().Connect(engine.OutShaft());

            debug("SimulationFactory connecting gearbox with auxiliary.");
            gearBox.InShaft().Connect(aux.OutShaft());

            debug("SimulationFactory connecting cycle with gearbox.");
            cycle.InShaft().Connect(gearBox.OutShaft());

            var dataWriter = new ModalDataWriter(resultFile);

            debug("Creating Simulator.");
            var simulator = new VectoSimulator(container, cycle, dataWriter);

            return simulator;
        }
    }
}
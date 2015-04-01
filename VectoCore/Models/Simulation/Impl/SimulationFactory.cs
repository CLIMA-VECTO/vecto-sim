using System;
using Common.Logging;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class SimulationFactory
    {
        public static IVectoJob CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile, string resultFile)
        {
            Action<string> debug = LogManager.GetLogger<SimulationFactory>().Debug;

            debug("SimulationFactory creating VehicleContainer.");
            var container = new VehicleContainer();

			debug("SimulationFactory creating cycle.");
			var cycleData = DrivingCycleData.ReadFromFileEngineOnly(cycleFile);
			var cycle = new EngineOnlyDrivingCycle(container, cycleData);
			
			debug("SimulationFactory creating engine.");
            var engineData = CombustionEngineData.ReadFromFile(engineFile);
            var engine = new CombustionEngine(container, engineData);

            debug("SimulationFactory creating gearbox.");
            var gearBox = new EngineOnlyGearbox(container);

			debug("SimulationFactory creating auxiliary");
	        var aux = new EngineOnlyAuxiliary(container, new AuxiliariesDemandAdapter(cycleData));

			debug("SimulationFactory connecting auxiliary with engine.");
			aux.Connect(engine.OutShaft());

			debug("SimulationFactory connecting gearbox with auxiliary.");
			gearBox.InShaft().Connect(aux.OutShaft());

            debug("SimulationFactory connecting cycle with gearbox.");
            cycle.InShaft().Connect(gearBox.OutShaft());

            var dataWriter = new ModalDataWriter(resultFile);

            debug("SimulationFactory creating VectoJob.");
            var job = new VectoJob(container, cycle, dataWriter);

            return job;
        }
    }
}
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class SimulationFactory
    {
        public static IVectoJob CreateTimeBasedEngineOnlyJob(string engineFile, string cycleFile, string resultFile)
        {
            var container = new VehicleContainer();
            var engineData = CombustionEngineData.ReadFromFile(engineFile);
            var engine = new CombustionEngine(container, engineData);
            var gearBox = new EngineOnlyGearbox(container);

            var engineOutShaft = engine.OutShaft();
            var gearBoxInShaft = gearBox.InShaft();

            gearBoxInShaft.Connect(engineOutShaft);

            var cycles = EngineOnlyDrivingCycle.ReadFromFile(cycleFile.ToString());
            var dataWriter = new ModalDataWriter(resultFile);

            var job = new EngineOnlyTimeBasedVectoJob(container, cycles, dataWriter);

            return job;
        }
    }
}
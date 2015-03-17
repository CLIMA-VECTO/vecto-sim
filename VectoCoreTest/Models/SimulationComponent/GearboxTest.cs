using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
    [TestClass]
    public class GearboxTest
    {
        [TestMethod]
        public void TestGearboxMemento()
        {
            var vehicle = new VehicleContainer();
            var origin = new Gearbox(vehicle);

            var data = Memento.Serialize(origin);

            var restored = Memento.Deserialize<Gearbox>(data);

            Assert.AreEqual(origin, restored);
        }
    }
}

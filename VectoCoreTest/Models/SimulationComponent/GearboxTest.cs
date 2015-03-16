using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var origin = new Gearbox();

            var data = Memento.Serialize(origin);

            var restored = Memento.Deserialize<Gearbox>(data);

            Assert.AreEqual(origin, restored);
        }
    }
}

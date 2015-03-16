using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
    [TestClass]
    public class WheelTest
    {
        [TestMethod]
        public void TestWheelsMemento()
        {
            var origin = new Wheels();

            var data = Memento.Serialize(origin);

            var restored = Memento.Deserialize<Wheels>(data);

            Assert.AreEqual(origin, restored);
        }
    }
}

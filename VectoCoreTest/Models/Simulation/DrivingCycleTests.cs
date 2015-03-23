using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Tests.Utils;

namespace TUGraz.VectoCore.Tests.Models.Simulation
{
    [TestClass]
    public class DrivingCycleTests
    {
        [TestMethod]
        public void TestEngineOnly()
        {
            var container = new VehicleContainer();

            var cycleData = DrivingCycleData.ReadFromFileEngineOnly(@"TestData\Cycles\Coach Engine Only.vdri");
            IEngineOnlyDrivingCycle cycle = new EngineOnlyDrivingCycle(container, cycleData);

            var outPort = new MockTnOutPort();
            ITnInPort inPort = cycle.InShaft();

            inPort.Connect(outPort);

            cycle.DoSimulationStep();

            var dataWriter = new TestModalDataWriter();
            container.CommitSimulationStep(dataWriter);

            Assert.AreEqual(0.0, outPort.AbsTime.TotalSeconds);
            Assert.AreEqual(1.0, outPort.Dt.TotalSeconds);
            Assert.AreEqual(1500, outPort.EngineSpeed);
            Assert.AreEqual(600, outPort.Torque);

            Assert.AreEqual(0.5, dataWriter[ModalResultField.time]);
        }

        [TestMethod]
        public void TestTimeBased()
        {
            var container = new VehicleContainer();

            var cycleData = DrivingCycleData.ReadFromFileTimeBased(@"TestData\Cycles\Coach.vdri");
            IDrivingCycle cycle = new TimeBasedDrivingCycle(container, cycleData);

            var outPort = new MockDriverDemandOutPort();

            IDriverDemandInPort inPort = cycle.InPort();

            inPort.Connect(outPort);

            cycle.DoSimulationStep();

            var dataWriter = new TestModalDataWriter();
            container.CommitSimulationStep(dataWriter);

            // todo: assert correct values!
            Assert.AreEqual(0.0, outPort.AbsTime.TotalSeconds);
            Assert.AreEqual(1.0, outPort.Dt.TotalSeconds);
            Assert.AreEqual(80, outPort.Velocity);
            Assert.AreEqual(0.03, outPort.Gradient);
            Assert.AreEqual(0.5, dataWriter[ModalResultField.time]);
        }

        [TestMethod]
        public void TestDistanceBased()
        {
            var container = new VehicleContainer();

            var cycleData = DrivingCycleData.ReadFromFileDistanceBased(@"TestData\Cycles\Coach.vdri");
            IDrivingCycle cycle = new DistanceBasedDrivingCycle(container, cycleData);

            var outPort = new MockDriverDemandOutPort();

            IDriverDemandInPort inPort = cycle.InPort();

            inPort.Connect(outPort);

            cycle.DoSimulationStep();

            var dataWriter = new TestModalDataWriter();
            container.CommitSimulationStep(dataWriter);

            // todo: assert correct values!
            Assert.AreEqual(0.0, outPort.AbsTime.TotalSeconds);
            Assert.AreEqual(1.0, outPort.Dt.TotalSeconds);
            Assert.AreEqual(80, outPort.Velocity);
            Assert.AreEqual(0.03, outPort.Gradient);
            Assert.AreEqual(0.5, dataWriter[ModalResultField.time]);
        }
    }
}

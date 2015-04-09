using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponent
{
  [TestClass]
  public class ClutchTest
  {
    private const string CoachEngine = @"TestData\Components\24t Coach.veng";

    [TestMethod]
    public void TestClutch()
    {
      var vehicle = new VehicleContainer();
      var engineData = CombustionEngineData.ReadFromFile(CoachEngine);


      var clutch = new Clutch(vehicle, engineData);

      ITnInPort inPort = clutch.InShaft();
      var outPort = new MockTnOutPort();

      inPort.Connect(outPort);

      ITnOutPort clutchOutPort = clutch.OutShaft();

      clutchOutPort.Request(new TimeSpan(), new TimeSpan(), 100.SI<NewtonMeter>(), 100.0.RPMtoRad());

      Assert.AreEqual(0, (double)outPort.Torque,0.001);
      Assert.AreEqual(0, (double)outPort.AngularFrequency, 0.001);

    }
  }
}

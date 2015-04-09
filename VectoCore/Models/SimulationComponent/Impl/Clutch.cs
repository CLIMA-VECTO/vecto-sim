using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
  public class Clutch:VectoSimulationComponent, IClutch,ITnOutPort,ITnInPort
  {
    public Clutch(IVehicleContainer cockpit, CombustionEngineData engineData) : base(cockpit)
    {

      //engineData.IdleSpeed;

      //engineData.GetFullLoadCurve(0).RatedSpeed();

    }
    public override void CommitSimulationStep(IModalDataWriter writer)
    {
      throw new NotImplementedException();
    }

    public ITnInPort InShaft()
    {
      throw new NotImplementedException();
    }

    public ITnOutPort OutShaft()
    {
      throw new NotImplementedException();
    }

    public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, RadianPerSecond angularVelocity)
    {
      throw new NotImplementedException();
    }

    public void Connect(ITnOutPort other)
    {
      throw new NotImplementedException();
    }
  }
}

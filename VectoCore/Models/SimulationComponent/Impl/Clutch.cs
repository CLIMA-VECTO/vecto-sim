using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
  public class Clutch : VectoSimulationComponent, IClutch, ITnOutPort, ITnInPort
  {
    private readonly PerSecond _idleSpeed;
    private readonly PerSecond _ratedSpeed;
    private ITnOutPort _nextComponent;
    private const double ClutchEff = 1;
    private const double CluchNormSpeed = 0.03;
	  public ClutchState _clutchState = ClutchState.ClutchClosed;

	  public enum ClutchState
    {
      ClutchClosed,
      ClutchOpened,
      ClutchSlipping
    }

    public Clutch(IVehicleContainer cockpit, CombustionEngineData engineData) : base(cockpit)
    {
      _idleSpeed = engineData.IdleSpeed;
      _ratedSpeed = engineData.GetFullLoadCurve(0).RatedSpeed();
    }

    public override void CommitSimulationStep(IModalDataWriter writer)
    {
      throw new NotImplementedException();
    }

    public ITnInPort InShaft()
    {
      return this;
    }


	  public ITnOutPort OutShaft()
	  {
		  return this;
	  }

    public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond angularVelocity)
    {
      var torque_in = new NewtonMeter();
      var engineSpeed_in = new PerSecond();
      var engineSpeed0 = new PerSecond();
      double engineSpeedNorm;


      if (Cockpit.Gear() == 0) {
        _clutchState = ClutchState.ClutchOpened;
        engineSpeed_in = _idleSpeed;
        torque_in = new NewtonMeter();
      } else {
        engineSpeedNorm = ((double) angularVelocity - (double) _idleSpeed) /
                          ((double) _ratedSpeed - (double) _idleSpeed);
        if (engineSpeedNorm < CluchNormSpeed) {
          _clutchState = ClutchState.ClutchSlipping;

          engineSpeed0 = new PerSecond(Math.Max((double) _idleSpeed, (double) angularVelocity));
          var clutchSpeedNorm = CluchNormSpeed /
                                ((_idleSpeed + CluchNormSpeed * (_ratedSpeed - _idleSpeed)) / _ratedSpeed);
          engineSpeed_in = new PerSecond((double)((clutchSpeedNorm * engineSpeed0 / _ratedSpeed) * (_ratedSpeed - _idleSpeed) + _idleSpeed));

          torque_in = Formulas.PowerToTorque(Formulas.TorqueToPower(torque,angularVelocity)/ClutchEff, engineSpeed_in); 

        } else {
          _clutchState = ClutchState.ClutchClosed;
          engineSpeed_in = angularVelocity;
          torque_in = torque;
        }
      }


      return _nextComponent.Request(absTime, dt, torque_in, engineSpeed_in);
    }

    public void Connect(ITnOutPort other)
    {
      _nextComponent = other;
    }
  }
}

using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class AxleGear : IPowerTrainComponent, ITnInPort, ITnOutPort
	{
		private ITnOutPort _nextComponent;
		private GearData _gearDataData;

		public AxleGear(GearData gearDataData)
		{
			_gearDataData = gearDataData;
		}

		public ITnInPort InShaft()
		{
			return this;
		}

		public ITnOutPort OutShaft()
		{
			return this;
		}

		public void Connect(ITnOutPort other)
		{
			_nextComponent = other;
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, NewtonMeter torque, PerSecond angularVelocity)
		{
			return _nextComponent.Request(absTime, dt, torque, angularVelocity);
		}
	}
}
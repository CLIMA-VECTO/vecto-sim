using System;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Brakes : VectoSimulationComponent, IPowerTrainComponent, ITnOutPort, ITnInPort, IBrakes
	{
		protected ITnOutPort NextComponent;

		public Brakes(IVehicleContainer dataBus) : base(dataBus) {}


		public ITnInPort InPort()
		{
			return this;
		}

		public ITnOutPort OutPort()
		{
			return this;
		}

		public Watt BrakePower { get; set; }

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			var brakeTorque = 0.SI<NewtonMeter>();
			if (!BrakePower.IsEqual(0)) {
				if (angularVelocity.IsEqual(0)) {
					brakeTorque = torque;
				} else {
					brakeTorque = BrakePower / angularVelocity;
				}
			}
			if (!dryRun && BrakePower < 0) {
				throw new VectoSimulationException("Negative Braking Power is not allowed!");
			}
			var retVal = NextComponent.Request(absTime, dt, torque + brakeTorque, angularVelocity, dryRun);
			retVal.BrakePower = brakeTorque * angularVelocity;
			return retVal;
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			BrakePower = 0.SI<Watt>();
			return DataBus.VehicleStopped
				? NextComponent.Initialize(0.SI<NewtonMeter>(), 0.SI<PerSecond>())
				: NextComponent.Initialize(torque, angularVelocity);
		}


		public void Connect(ITnOutPort other)
		{
			NextComponent = other;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Pbrake] = BrakePower;
		}

		protected override void DoCommitSimulationStep()
		{
			BrakePower = 0.SI<Watt>();
		}
	}
}
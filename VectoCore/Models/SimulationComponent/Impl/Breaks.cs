using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Breaks : VectoSimulationComponent, IPowerTrainComponent, ITnOutPort, ITnInPort, IBreaks
	{
		protected ITnOutPort Next;

		protected NewtonMeter BreakTorque;

		public Breaks(IVehicleContainer dataBus) : base(dataBus) {}


		public ITnInPort InPort()
		{
			return this;
		}

		public ITnOutPort OutPort()
		{
			return this;
		}

		public Watt BreakPower { get; set; }

		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun = false)
		{
			if (!BreakPower.IsEqual(0)) {
				if (angularVelocity.IsEqual(0)) {
					BreakTorque = torque;
				} else {
					BreakTorque = Formulas.PowerToTorque(BreakPower, angularVelocity);
				}
			}
			return Next.Request(absTime, dt, torque - BreakTorque, angularVelocity, dryRun);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond angularVelocity)
		{
			BreakPower = 0.SI<Watt>();
			BreakTorque = 0.SI<NewtonMeter>();
			return Next.Initialize(torque, angularVelocity);
		}


		public void Connect(ITnOutPort other)
		{
			Next = other;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			writer[ModalResultField.Pbrake] = BreakPower;
		}

		protected override void DoCommitSimulationStep()
		{
			BreakPower = 0.SI<Watt>();
			BreakTorque = 0.SI<NewtonMeter>();
		}
	}
}
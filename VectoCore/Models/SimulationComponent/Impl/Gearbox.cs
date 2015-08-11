using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort, ITnInPort
	{
		protected ITnOutPort Next;
		private uint _gear;

		internal GearboxData Data;

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
			_gear = 1;
		}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region ITnOutProvider

		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IGearboxCockpit

		uint IGearboxInfo.Gear()
		{
			return _gear;
		}

		#endregion

		#region ITnOutPort

		private GearData CurrentGear
		{
			get { return Data.Gears[_gear]; }
		}


		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			// todo check fulloadcurve for overload


			// todo check shiftpolygon for shifting


			engineSpeed *= CurrentGear.Ratio;
			torque = CurrentGear.LossMap.GearboxInTorque(engineSpeed, torque);

			return Next.Request(absTime, dt, torque, engineSpeed);
		}

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			_gear = 1;
			return Next.Initialize(torque, engineSpeed);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			Next = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			//todo implement
		}

		protected override void DoCommitSimulationStep()
		{
			//todo implement
		}

		#endregion
	}
}
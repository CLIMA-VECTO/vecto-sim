using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
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

		internal uint Gear
		{
			get { return _gear; }
			set { _gear = value; }
		}


		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed, bool dryRun)
		{
			bool gearChanged;
			PerSecond inEngineSpeed;
			NewtonMeter inTorque;

			do {
				gearChanged = false;
				// calculate new inEngineSpeed and Torque for the current gear
				inEngineSpeed = engineSpeed * CurrentGear.Ratio;
				inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, torque);

				//todo: 
				// Data.TorqueReserve ... % TorqueReserver for GearSkipping and EarlyUpshift
				// Data.ShiftTime ... minimal time between shift operations
				// AT, MT, AMT .... different behaviour!

				// check if GearBox should shift up
				if (_gear < Data.Gears.Count) {
					var upSection = CurrentGear.ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (inEngineSpeed > upSection.Item2.AngularSpeed ||
						(inEngineSpeed > upSection.Item1.AngularSpeed &&
						(inTorque < upSection.Item1.Torque || inTorque < upSection.Item2.Torque))) {
						_gear++;
						gearChanged = true;
						continue;
					}
				}

				// check if GearBox should shift down
				if (_gear > 1) {
					var downSection = CurrentGear.ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (inEngineSpeed < downSection.Item1.AngularSpeed ||
						(inEngineSpeed < downSection.Item2.AngularSpeed &&
						(inTorque > downSection.Item1.Torque || inTorque > downSection.Item2.Torque))) {
						_gear--;
						gearChanged = true;
					}
				}
			} while (Data.SkipGears && gearChanged);

			if (gearChanged) {
				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			// check full load curve for overload/underload (mirrored)
			var maxTorque = CurrentGear.FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed);
			if (inTorque.Abs() > maxTorque) {
				return new ResponseFailOverload {
					GearboxPowerRequest = inTorque * inEngineSpeed,
					Delta = Math.Sign(inTorque.Value()) * (inTorque.Abs() - maxTorque) * inEngineSpeed
				};
			}

			return Next.Request(absTime, dt, inTorque, inEngineSpeed);
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

	internal class ResponseGearShift : AbstractResponse
	{
		public override ResponseType ResponseType
		{
			get { return ResponseType.GearShift; }
		}
	}
}
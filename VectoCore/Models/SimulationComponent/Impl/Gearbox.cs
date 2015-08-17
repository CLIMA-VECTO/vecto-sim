using System;
using System.Diagnostics;
using TUGraz.VectoCore.Exceptions;
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
			_gear = 0;
		}

		#region ITnInProvider

		public ITnInPort InPort()
		{
			return this;
		}

		#endregion

		#region ITnOutProvider

		[DebuggerHidden]
		public ITnOutPort OutPort()
		{
			return this;
		}

		#endregion

		#region IGearboxCockpit

		uint IGearboxInfo.Gear
		{
			get { return _gear; }
			set { _gear = value; }
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


		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			if (Gear == 0 || outEngineSpeed.IsEqual(0)) {
				return Next.Request(absTime, dt, 0.SI<NewtonMeter>(), 0.SI<PerSecond>());
			}

			bool gearChanged;
			PerSecond inEngineSpeed;
			NewtonMeter inTorque;

			var previousGear = _gear;

			do {
				gearChanged = false;
				// calculate new inEngineSpeed and Torque for the current gear
				inEngineSpeed = outEngineSpeed * CurrentGear.Ratio;
				inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, outTorque);

				//todo: 
				// Data.TorqueReserve ... % TorqueReserver for GearSkipping and EarlyUpshift
				// Data.ShiftTime ... minimal time between shift operations
				// AT, MT, AMT .... different behaviour!

				// check if GearBox should shift up
				if (_gear < Data.Gears.Count) {
					var upSection = CurrentGear.ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (IsRight(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2)) {
						_gear++;
						gearChanged = true;
						continue;
					}
				}

				// check if GearBox should shift down
				if (_gear > 1) {
					var downSection = CurrentGear.ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (IsLeft(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2)) {
						_gear--;
						gearChanged = true;
					}
				}
			} while (Data.SkipGears && gearChanged);

			// check full load curve for overload/underload (mirrored)
			var maxTorque = CurrentGear.FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed);
			if (inTorque.Abs() > maxTorque) {
				return new ResponseFailOverload {
					GearboxPowerRequest = inTorque * inEngineSpeed,
					Delta = Math.Sign(inTorque.Value()) * (inTorque.Abs() - maxTorque) * inEngineSpeed
				};
			}

			if (previousGear != _gear) {
				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			return Next.Request(absTime, dt, inTorque, inEngineSpeed);
		}

		/// <summary>
		/// Determines whether the given point is left or right from the line given by (from, to)
		/// </summary>
		/// <remarks>Calculates the 2d cross product (from, to) x (from, [angularSpeed, torque]) and checks if the z-component is positive or negative.</remarks>
		/// <param name="angularSpeed">The angular speed.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		private static bool IsLeft(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			return ((to.AngularSpeed - from.AngularSpeed) * (torque - from.Torque) -
					(to.Torque - from.Torque) * (angularSpeed - from.AngularSpeed)) >= 0;
		}

		/// <summary>
		/// Determines whether the given point is left or right from the line given by (from, to)
		/// </summary>
		/// <remarks>Calculates the 2d cross product (from, to) x (from, [angularSpeed, torque]) and checks if the z-component is positive or negative.</remarks>
		/// <param name="angularSpeed">The angular speed.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns></returns>
		private static bool IsRight(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			return ((to.AngularSpeed - from.AngularSpeed) * (torque - from.Torque) -
					(to.Torque - from.Torque) * (angularSpeed - from.AngularSpeed)) <= 0;
		}


		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			_gear = 0;
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
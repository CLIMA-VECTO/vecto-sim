using System;
using System.Diagnostics;
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
		private Second _lastShiftTime = double.NegativeInfinity.SI<Second>();

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
			_gear = 0;
		}

		private GearData CurrentGear
		{
			get { return Data.Gears[_gear]; }
		}

		internal uint Gear
		{
			get { return _gear; }
			set { _gear = value; }
		}


		private static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var p = new Point(angularSpeed.Value(), torque.Value());
			var edge = new Edge(new Point(from.AngularSpeed.Value(), from.Torque.Value()),
				new Point(to.AngularSpeed.Value(), to.Torque.Value()));
			return p.IsLeftOf(edge);
		}

		private static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var p = new Point(angularSpeed.Value(), torque.Value());
			var edge = new Edge(new Point(from.AngularSpeed.Value(), from.Torque.Value()),
				new Point(to.AngularSpeed.Value(), to.Torque.Value()));
			return p.IsRightOf(edge);
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

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			// TODO:
			// * EarlyUpshift (shift before outside of up-shift curve if torque reserve for the next higher gear is fullfilled)
			// * SkipGears (when already shifting to next gear, check if torque reserve is fullfilled for the overnext gear and eventually shift to it)
			// * AT, MT, AMT .... different behaviour!

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

				// check last shift time if a new shift is allowed
				if (absTime - _lastShiftTime < Data.ShiftTime) {
					continue;
				}

				// check if GearBox should shift up
				if (_gear < Data.Gears.Count) {
					var upSection = CurrentGear.ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (IsOnRightSide(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2)) {
						_gear++;
						gearChanged = true;
						continue;
					}
				}

				// check if GearBox should shift down
				if (_gear > 1) {
					var downSection = CurrentGear.ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
					if (IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2)) {
						_gear--;
						gearChanged = true;
					}
				}
			} while (Data.SkipGears && gearChanged);

			// check full load curve for overload/underload (mirrored with Abs() )
			var maxTorque = CurrentGear.FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed);
			if (inTorque.Abs() > maxTorque) {
				return new ResponseFailOverload {
					GearboxPowerRequest = inTorque * inEngineSpeed,
					Delta = Math.Sign(inTorque.Value()) * (inTorque.Abs() - maxTorque) * inEngineSpeed
				};
			}

			// GearShift Response
			if (previousGear != _gear) {
				_lastShiftTime = absTime;
				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			return Next.Request(absTime, dt, inTorque, inEngineSpeed);
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
}
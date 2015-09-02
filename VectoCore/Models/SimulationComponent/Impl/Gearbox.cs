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

		internal GearboxData Data;

		private uint _gear;
		private Second _lastShiftTime = double.NegativeInfinity.SI<Second>();
		private uint _previousGear;
		private Watt _loss;

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
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

		public IResponse Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			// TODO:
			// * EarlyUpshift (shift before outside of up-shift curve if torque reserve for the next higher gear is fullfilled)
			// * SkipGears (when already shifting to next gear, check if torque reserve is fullfilled for the overnext gear and eventually shift to it)
			// * MT, AMT and AT .... different behaviour!

			//Special Behaviour: When Gear is 0 (no gear set) OR the speed is 0 (not rotating) a zero-request is applied.
			if (Gear == 0) {
				if (dryRun) {
					return new ResponseDryRun() { GearboxPowerRequest = outTorque * outEngineSpeed };
				}
				var resp = Next.Request(absTime, dt, 0.SI<NewtonMeter>(), 0.SI<PerSecond>(), dryRun);
				resp.GearboxPowerRequest = outTorque * outEngineSpeed;
				return resp;
			}

			_previousGear = _gear;

			var inEngineSpeed = outEngineSpeed * CurrentGear.Ratio;
			var inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			var gearChanged = true;
			while (IsShiftAllowed(absTime, dryRun) && gearChanged) {
				gearChanged = false;
				if (ShouldShiftUp(inEngineSpeed, inTorque)) {
					_gear++;
					gearChanged = true;
				} else if (ShouldShiftDown(inEngineSpeed, inTorque)) {
					_gear--;
					gearChanged = true;
				}

				if (gearChanged) {
					inEngineSpeed = outEngineSpeed * CurrentGear.Ratio;
					inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, outTorque);
					if (!Data.SkipGears) {
						break;
					}
				}
			}

			_loss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;

			// DryRun Response, gear != 0
			if (dryRun) {
				var r = Next.Request(absTime, dt, inTorque, inEngineSpeed, true);
				r.GearboxPowerRequest = outTorque * outEngineSpeed;
				return r;
			}

			// Overload Response
			if (CurrentGear.FullLoadCurve != null) {
				var maxTorque = CurrentGear.FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed);

				if (inEngineSpeed > 0 && inTorque.Abs() > maxTorque) {
					_gear = _previousGear;
					return new ResponseGearboxOverload {
						Delta = Math.Sign(inTorque.Value()) * (inTorque.Abs() - maxTorque) * inEngineSpeed,
						GearboxPowerRequest = inTorque * inEngineSpeed,
					};
				}
			}

			// GearShift Response
			if (_previousGear != _gear) {
				_lastShiftTime = absTime;
				Log.Debug("Gearbox Shift from gear {0} to gear {1}.", _previousGear, _gear);
				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			// Normal Response
			var response = Next.Request(absTime, dt, inTorque, inEngineSpeed);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;

			return response;
		}


		/// <summary>
		/// Tests if a shift is allowed.
		/// </summary>
		private bool IsShiftAllowed(Second absTime, bool dryRun)
		{
			return !dryRun && absTime - _lastShiftTime >= Data.ShiftTime;
		}

		/// <summary>
		/// Tests if the gearbox should shift down.
		/// </summary>
		private bool ShouldShiftDown(PerSecond inEngineSpeed, NewtonMeter inTorque)
		{
			if (_gear <= 1) {
				return false;
			}

			var downSection = CurrentGear.ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2);
		}

		/// <summary>
		/// Tests if the gearbox should shift up.
		/// </summary>
		private bool ShouldShiftUp(PerSecond inEngineSpeed, NewtonMeter inTorque)
		{
			if (_gear >= Data.Gears.Count) {
				return false;
			}

			var upSection = CurrentGear.ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnRightSide(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		private static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var p = new Point(angularSpeed.Value(), torque.Value());
			var edge = new Edge(new Point(from.AngularSpeed.Value(), from.Torque.Value()),
				new Point(to.AngularSpeed.Value(), to.Torque.Value()));
			return p.IsLeftOf(edge);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		private static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var p = new Point(angularSpeed.Value(), torque.Value());
			var edge = new Edge(new Point(from.AngularSpeed.Value(), from.Torque.Value()),
				new Point(to.AngularSpeed.Value(), to.Torque.Value()));
			return p.IsRightOf(edge);
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
			writer[ModalResultField.Gear] = _gear;
			writer[ModalResultField.PlossGB] = _loss;

			// todo Gearbox PaGB rotational acceleration power in moddata
			writer[ModalResultField.PaGB] = 0.SI();
		}

		protected override void DoCommitSimulationStep()
		{
			_previousGear = _gear;
			_loss = 0.SI<Watt>();
		}

		#endregion
	}
}
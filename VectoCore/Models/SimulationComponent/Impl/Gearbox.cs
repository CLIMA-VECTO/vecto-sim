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

		IResponse ITnOutPort.Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			// TODO:
			// * EarlyUpshift (shift before outside of up-shift curve if torque reserve for the next higher gear is fullfilled)
			// * SkipGears (when already shifting to next gear, check if torque reserve is fullfilled for the overnext gear and eventually shift to it)
			// * MT, AMT and AT .... different behaviour!

			//Special Behaviour: When Gear is 0 (no gear set) or the speed is 0 (not rotating) a zero-request is applied.
			if (Gear == 0 || outEngineSpeed.IsEqual(0)) {
				return Next.Request(absTime, dt, 0.SI<NewtonMeter>(), 0.SI<PerSecond>(), dryRun);
			}

			bool gearChanged;
			PerSecond inEngineSpeed;
			NewtonMeter inTorque;

			_previousGear = _gear;

			do {
				gearChanged = false;

				inEngineSpeed = outEngineSpeed * CurrentGear.Ratio;
				inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, outTorque);
				_loss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;

				if (!ShiftAllowed(absTime)) {
					continue;
				}

				if (ShouldShiftUp(inEngineSpeed, inTorque)) {
					_gear++;
					gearChanged = true;
					continue;
				}

				if (ShouldShiftDown(inEngineSpeed, inTorque)) {
					_gear--;
					gearChanged = true;
				}
			} while (Data.SkipGears && gearChanged);

			// Overload Response
			var maxTorque = CurrentGear.FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed);
			if (inTorque.Abs() > maxTorque) {
				_gear = _previousGear;
				return new ResponseGearboxOverload {
					Delta = (inTorque.Abs() - maxTorque) * inEngineSpeed,
					GearboxPowerRequest = inTorque * inEngineSpeed,
				};
			}

			// GearShift Response
			if (_previousGear != _gear) {
				_lastShiftTime = absTime;
				Log.DebugFormat("Gearbox Shift from gear {0} to gear {0}.", _previousGear, _gear);
				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			var response = Next.Request(absTime, dt, inTorque, inEngineSpeed, dryRun);
			response.GearboxPowerRequest = inTorque * inEngineSpeed;

			response.Switch().
				Case<ResponseDryRun>(r => r.GearboxDeltaFullLoad = (maxTorque - inTorque) * inEngineSpeed);

			return response;
		}


		/// <summary>
		/// Tests if a shift is allowed.
		/// </summary>
		private bool ShiftAllowed(Second absTime)
		{
			return absTime - _lastShiftTime >= Data.ShiftTime;
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
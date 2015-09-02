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
		private Second _shiftTime = double.NegativeInfinity.SI<Second>();
		private uint _previousGear;
		private Watt _powerLoss;

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

			if (_gear == 0) {
				// if no gear is set and dry run: just set GearBoxPowerRequest
				if (dryRun) {
					return new ResponseDryRun { GearboxPowerRequest = outTorque * outEngineSpeed };
				}

				// if shiftTime still not reached (only happens during shifting): apply zero-request
				if (_shiftTime > absTime) {
					var duringShiftResponse = Next.Request(absTime, dt, 0.SI<NewtonMeter>(), 0.SI<PerSecond>());
					duringShiftResponse.GearboxPowerRequest = outTorque * outEngineSpeed;
					return duringShiftResponse;
				}

				// if shiftTime was reached and gear is not set: set correct gear
				if (_shiftTime <= absTime) {
					_gear = FindGear(outTorque, outEngineSpeed);
				}
			}

			var inEngineSpeed = outEngineSpeed * CurrentGear.Ratio;
			var inTorque = CurrentGear.LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			// if dryRun and gear is set: apply dryRun request
			if (dryRun) {
				var dryRunResponse = Next.Request(absTime, dt, inTorque, inEngineSpeed, true);
				dryRunResponse.GearboxPowerRequest = outTorque * outEngineSpeed;
				return dryRunResponse;
			}

			// Check if shift is needed and eventually return ResponseGearShift
			if (_shiftTime + Data.ShiftTime < absTime &&
				(ShouldShiftUp(inEngineSpeed, inTorque) || ShouldShiftDown(inEngineSpeed, inTorque))) {
				_shiftTime = absTime + Data.TractionInterruption;
				_gear = 0;

				Log.Debug("Gearbox is shifting. absTime: {0}, shiftTime: {1}, outTorque:{2}, outEngineSpeed: {3}",
					absTime, _shiftTime, outTorque, outEngineSpeed);

				return new ResponseGearShift { SimulationInterval = Data.TractionInterruption };
			}

			// Normal Response
			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;
			var response = Next.Request(absTime, dt, inTorque, inEngineSpeed);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;
			return response;
		}

		/// <summary>
		/// Finds the correct gear for current torque and engineSpeed request.
		/// </summary>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outEngineSpeed">The out engine speed.</param>
		/// <returns></returns>
		private uint FindGear(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			uint gear = 1;
			var inEngineSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
			var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			do {
				if (ShouldShiftUp(inEngineSpeed, inTorque)) {
					gear++;
					continue;
				}

				if (ShouldShiftDown(inEngineSpeed, inTorque)) {
					gear--;
					continue;
				}
				break;
			} while (Data.SkipGears);
			return gear;
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
			writer[ModalResultField.PlossGB] = _powerLoss;

			// todo Gearbox PaGB rotational acceleration power in moddata
			writer[ModalResultField.PaGB] = 0.SI();
		}

		protected override void DoCommitSimulationStep()
		{
			_previousGear = _gear;
			_powerLoss = 0.SI<Watt>();
		}

		#endregion
	}
}
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
		/// <summary>
		/// The next port.
		/// </summary>
		protected ITnOutPort Next;

		/// <summary>
		/// The data and settings for the gearbox.
		/// </summary>
		internal GearboxData Data;

		/// <summary>
		/// Time when a gearbox shift is finished. Is set when shifting is needed.
		/// </summary>
		private Second _shiftTime = double.NegativeInfinity.SI<Second>();

		/// <summary>
		/// The power loss for the mod data.
		/// </summary>
		private Watt _powerLoss;

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
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

		/// <summary>
		/// The current gear.
		/// </summary>
		public uint Gear { get; set; }

		#endregion

		#region ITnOutPort

		public IResponse Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			// TODO:
			// * EarlyUpshift (shift before outside of up-shift curve if torque reserve for the next higher gear is fullfilled)
			// * SkipGears (when already shifting to next gear, check if torque reserve is fullfilled for the overnext gear and eventually shift to it)
			// * MT, AMT and AT .... different behaviour!

			if (Gear == 0) {
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
					Gear = FindGear(outTorque, outEngineSpeed);
				}
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

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
				Gear = 0;

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
			var gear = (Gear != 0) ? Gear : 1;

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
			if (Gear <= 1) {
				return false;
			}

			var downSection = Data.Gears[Gear].ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2);
		}

		/// <summary>
		/// Tests if the gearbox should shift up.
		/// </summary>
		private bool ShouldShiftUp(PerSecond inEngineSpeed, NewtonMeter inTorque)
		{
			if (Gear >= Data.Gears.Count) {
				return false;
			}

			var upSection = Data.Gears[Gear].ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
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
			Gear = FindGear(torque, engineSpeed);

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
			writer[ModalResultField.Gear] = Gear;
			writer[ModalResultField.PlossGB] = _powerLoss;

			// todo Gearbox PaGB rotational acceleration power in moddata
			writer[ModalResultField.PaGB] = 0.SI();
		}

		protected override void DoCommitSimulationStep()
		{
			_powerLoss = 0.SI<Watt>();
		}

		#endregion
	}
}
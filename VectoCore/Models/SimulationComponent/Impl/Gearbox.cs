using System.Diagnostics;
using TUGraz.VectoCore.Configuration;
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
	public class Gearbox : VectoSimulationComponent, IGearbox, ITnOutPort, ITnInPort, IClutchInfo
	{
		/// <summary>
		/// The next port.
		/// </summary>
		protected ITnOutPort NextComponent;

		/// <summary>
		/// The data and settings for the gearbox.
		/// </summary>
		internal GearboxData Data;

		/// <summary>
		/// Time when a gearbox shift is finished. Is set when shifting is needed.
		/// </summary>
		private Second _shiftTime = double.NegativeInfinity.SI<Second>();

		//private Second _lastShiftTime = double.NegativeInfinity.SI<Second>();

		/// <summary>
		/// The power loss for the mod data.
		/// </summary>
		private Watt _powerLoss;

		public bool ClutchClosed(Second absTime)
		{
			return _shiftTime <= absTime; //  && Gear != 0;
		}

		public Gearbox(IVehicleContainer container, GearboxData gearboxData) : base(container)
		{
			Data = gearboxData;
			LastGear = 1;
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

		protected uint LastGear;

		#region ITnOutPort

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			_shiftTime = double.NegativeInfinity.SI<Second>();
			Gear = FindGear(torque, engineSpeed, true);

			return NextComponent.Initialize(torque, engineSpeed);
		}

		/// <summary>
		/// Requests the Gearbox to deliver outTorque and outEngineSpeed
		/// </summary>
		/// <returns>
		/// Special Cases:
		/// * ResponseDryRun
		/// * ResponseOverload
		/// * ResponseGearshift
		/// </returns>
		public IResponse Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			// TODO:
			// * EarlyUpshift (shift before outside of up-shift curve if torque reserve for the next higher gear is fullfilled)
			// * SkipGears (when already shifting to next gear, check if torque reserve is fullfilled for the overnext gear and eventually shift to it)
			// * MT, AMT and AT .... different behaviour!

			var shiftTimeExceeded = absTime.IsSmaller(_shiftTime) && _shiftTime.IsSmaller(absTime + dt);
			if (!dryRun && shiftTimeExceeded) {
				return new ResponseFailTimeInterval {
					DeltaT = _shiftTime - absTime,
					GearboxPowerRequest = outTorque * outEngineSpeed,
					Gear = Gear,
					Source = this
				};
			}
			if (DataBus.VehicleSpeed.IsEqual(0)) {
				Gear = FindGear(outTorque, outEngineSpeed, Data.SkipGears);
			}
			var retVal = absTime.IsSmaller(_shiftTime)
				? DoHandleRequestNeutralGear(absTime, dt, outTorque, outEngineSpeed, dryRun)
				: DoHandleRequestGearEngaged(absTime, dt, outTorque, outEngineSpeed, dryRun);
			retVal.Gear = Gear;
			return retVal;
		}

		private IResponse DoHandleRequestNeutralGear(Second absTime, Second dt, NewtonMeter outTorque,
			PerSecond outEngineSpeed, bool dryRun)
		{
			if (dryRun) {
				return new ResponseDryRun { GearboxPowerRequest = outTorque * outEngineSpeed, Source = this };
			}

			if (!outTorque.IsEqual(0, Constants.SimulationSettings.EngineFLDPowerTolerance)) {
				if (outTorque > 0) {
					return new ResponseOverload {
						Delta = outTorque * outEngineSpeed,
						Source = this,
						GearboxPowerRequest = outTorque * outEngineSpeed
					};
				}
				return new ResponseUnderload {
					Delta = outTorque * outEngineSpeed,
					Source = this,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}

			Log.Debug("Current Gear: Neutral");
			var neutralResponse = NextComponent.Request(absTime, dt, 0.SI<NewtonMeter>(), null);
			neutralResponse.GearboxPowerRequest = outTorque * outEngineSpeed;

			return neutralResponse;
		}

		private IResponse DoHandleRequestGearEngaged(Second absTime, Second dt, NewtonMeter outTorque,
			PerSecond outEngineSpeed, bool dryRun)
		{
			if (Gear == 0) {
				Gear = FindGear(outTorque, outEngineSpeed, Data.SkipGears);
				//_lastShiftTime = _shiftTime;
				//_shiftTime = double.NegativeInfinity.SI<Second>();
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			// Check if shift is needed and eventually return ResponseGearShift
			if (!dryRun && _shiftTime + Data.ShiftTime < absTime &&
				(ShouldShiftUp(Gear, inEngineSpeed, inTorque) || ShouldShiftDown(Gear, inEngineSpeed, inTorque))) {
				_shiftTime = absTime + Data.TractionInterruption;
				LastGear = FindGear(outTorque, outEngineSpeed, Data.SkipGears);
				//NextGear = FindGear(outTorque, outEngineSpeed, Data.SkipGears)
				Gear = 0;

				Log.Debug("Gearbox is shifting. absTime: {0}, shiftTime: {1}, outTorque:{2}, outEngineSpeed: {3}",
					absTime, _shiftTime, outTorque, outEngineSpeed);

				return new ResponseGearShift {
					SimulationInterval = Data.TractionInterruption,
					Source = this,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}
			Log.Debug("Current Gear: {0}", Gear);

			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;
			var response = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed, dryRun);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;
			return response;
		}

		/// <summary>
		/// Finds the correct gear for current torque and engineSpeed request.
		/// </summary>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outEngineSpeed">The out engine speed.</param>
		/// <param name="allowSkipGears"></param>
		/// <returns></returns>
		private uint FindGear(NewtonMeter outTorque, PerSecond outEngineSpeed, bool allowSkipGears)
		{
			var gear = (Gear != 0) ? Gear : LastGear;

			do {
				var inEngineSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
				var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);
				if (ShouldShiftUp(gear, inEngineSpeed, inTorque)) {
					gear++;
					continue;
				}

				if (ShouldShiftDown(gear, inEngineSpeed, inTorque)) {
					gear--;
					continue;
				}
				break;
				// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
			} while (allowSkipGears);
			if (gear == 0) {
				throw new VectoSimulationException(
					"Could not find gear! Gear: {0}, LastGear: {1}, outTorque: {2}, outEngineSpeed: {3}, skipGears: {4}",
					Gear, LastGear, outTorque, outEngineSpeed, allowSkipGears);
			}
			return gear;
		}

		/// <summary>
		/// Tests if the gearbox should shift down.
		/// </summary>
		private bool ShouldShiftDown(uint gear, PerSecond inEngineSpeed, NewtonMeter inTorque)
		{
			if (gear <= 1) {
				return false;
			}

			var downSection = Data.Gears[gear].ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2);
		}

		/// <summary>
		/// Tests if the gearbox should shift up.
		/// </summary>
		private bool ShouldShiftUp(uint gear, PerSecond inEngineSpeed, NewtonMeter inTorque)
		{
			if (gear >= Data.Gears.Count) {
				return false;
			}

			var upSection = Data.Gears[gear].ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnRightSide(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		private static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).Value().IsGreater(0);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		private static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).Value().IsSmaller(0);
		}

		#endregion

		#region ITnInPort

		void ITnInPort.Connect(ITnOutPort other)
		{
			NextComponent = other;
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
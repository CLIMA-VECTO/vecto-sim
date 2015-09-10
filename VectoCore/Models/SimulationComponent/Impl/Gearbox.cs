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

// TODO:
// * EarlyUpshift (shift before outside of up-shift curve if outTorque reserve for the next higher gear is fullfilled)
// * SkipGears (when already shifting to next gear, check if outTorque reserve is fullfilled for the overnext gear and eventually shift to it)

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
		/// The shift strategy.
		/// </summary>
		private readonly IShiftStrategy _strategy;

		/// <summary>
		/// Time when a gearbox shift engages a new gear (shift is finished). Is set when shifting is needed.
		/// </summary>
		private Second _shiftTime = double.NegativeInfinity.SI<Second>();

		/// <summary>
		/// The power loss for the mod data.
		/// </summary>
		private Watt _powerLoss;

		private bool _disengaged = true;


		public bool ClutchClosed(Second absTime)
		{
			return _shiftTime.IsSmaller(absTime);
		}

		public Gearbox(IVehicleContainer container, GearboxData gearboxData, IShiftStrategy strategy = null) : base(container)
		{
			// TODO: no default strategy! gearbox has to be called with explicit shift strategy!
			if (strategy == null) {
				strategy = new AMTShiftStrategy(gearboxData);
			}
			Data = gearboxData;
			_strategy = strategy;
			_strategy.Gearbox = this;
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

		public IResponse Initialize(NewtonMeter torque, PerSecond engineSpeed)
		{
			_shiftTime = double.NegativeInfinity.SI<Second>();
			Gear = _strategy.InitGear(torque, engineSpeed);
			_disengaged = true;
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
			var retVal = ClutchClosed(absTime)
				? RequestGearEngaged(absTime, dt, outTorque, outEngineSpeed, dryRun)
				: RequestGearDisengaged(absTime, dt, outTorque, outEngineSpeed, dryRun);

			return retVal;
		}

		private IResponse RequestGearDisengaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed,
			bool dryRun)
		{
			Log.Debug("Current Gear: Neutral");

			if (dryRun) {
				return new ResponseDryRun { GearboxPowerRequest = outTorque * outEngineSpeed, Source = this };
			}

			var shiftTimeExceeded = absTime.IsSmaller(_shiftTime) && _shiftTime.IsSmaller(absTime + dt);
			if (shiftTimeExceeded) {
				return new ResponseFailTimeInterval {
					Source = this,
					DeltaT = _shiftTime - absTime,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}

			if (outTorque.IsGreater(0, Constants.SimulationSettings.EnginePowerSearchTolerance)) {
				return new ResponseOverload {
					Source = this,
					Delta = outTorque * outEngineSpeed,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}

			if (outTorque.IsSmaller(0, Constants.SimulationSettings.EnginePowerSearchTolerance)) {
				return new ResponseUnderload {
					Source = this,
					Delta = outTorque * outEngineSpeed,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}

			if (DataBus.VehicleSpeed.IsEqual(0)) {
				Gear = _strategy.Engage(outTorque, outEngineSpeed);
			}

			var response = NextComponent.Request(absTime, dt, 0.SI<NewtonMeter>(), null);

			response.GearboxPowerRequest = outTorque * outEngineSpeed;

			return response;
		}

		private IResponse RequestGearEngaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed,
			bool dryRun)
		{
			// Engage a Gear if no gear set
			if (_disengaged) {
				Gear = _strategy.Engage(outTorque, outEngineSpeed);
				_disengaged = false;
				Log.Debug("Gearbox engaged gear {0}", Gear);
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			if (dryRun) {
				var dryRunResponse = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed, true);
				dryRunResponse.GearboxPowerRequest = outTorque * outEngineSpeed;
				return dryRunResponse;
			}

			// Check if shift is needed and eventually return ResponseGearShift
			var isShiftAllowed = (_shiftTime + Data.ShiftTime).IsSmaller(absTime);
			if (isShiftAllowed && _strategy.ShiftRequired(Gear, inTorque, inEngineSpeed)) {
				_shiftTime = absTime + Data.TractionInterruption;

				_disengaged = true;
				_strategy.Disengage(outTorque, outEngineSpeed);
				Log.Info("Gearbox disengaged");

				Log.Debug("Gearbox is shifting. absTime: {0}, shiftTime: {1}, outTorque:{2}, outEngineSpeed: {3}",
					absTime, _shiftTime, outTorque, outEngineSpeed);

				return new ResponseGearShift {
					Source = this,
					SimulationInterval = Data.TractionInterruption,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
			}

			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;
			var response = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;
			return response;
		}

		#endregion

		#region ITnInPort

		void ITnInPort.
			Connect(ITnOutPort
				other)
		{
			NextComponent = other;
		}

		#endregion

		#region VectoSimulationComponent

		protected override
			void DoWriteModalResults
			(IModalDataWriter
				writer)
		{
			writer[ModalResultField.Gear] = Gear;
			writer[ModalResultField.PlossGB] = _powerLoss;

			// todo Gearbox PaGB rotational acceleration power in moddata
			writer[ModalResultField.PaGB] = 0.SI();
		}

		protected override
			void DoCommitSimulationStep
			()
		{
			_powerLoss = 0.SI<Watt>();
		}

		#endregion
	}

	public interface IShiftStrategy
	{
		bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		uint InitGear(NewtonMeter torque, PerSecond angularSpeed);
		uint Engage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		Gearbox Gearbox { get; set; }
	}


	public abstract class ShiftStrategy : IShiftStrategy
	{
		public abstract uint Engage(NewtonMeter torque, PerSecond angularSpeed);
		public abstract void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		public abstract uint InitGear(NewtonMeter torque, PerSecond angularSpeed);

		protected GearboxData Data;
		public Gearbox Gearbox { get; set; }


		protected ShiftStrategy(GearboxData data)
		{
			Data = data;
		}

		/// <summary>
		/// Tests if the gearbox should shift down.
		/// </summary>
		protected bool ShouldShiftDown(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
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
		protected bool ShouldShiftUp(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
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
		protected static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).IsGreater(0);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		protected static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).IsSmaller(0);
		}
	}


	public class AMTShiftStrategy : ShiftStrategy
	{
		protected uint LastGear;

		public AMTShiftStrategy(GearboxData data) : base(data)
		{
			LastGear = 1;
		}

		public uint GetGear(uint gear, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			do {
				var inEngineSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
				var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);
				if (ShouldShiftUp(gear, inTorque, inEngineSpeed)) {
					gear++;
					continue;
				}

				if (ShouldShiftDown(gear, inTorque, inEngineSpeed)) {
					gear--;
					continue;
				}
				break;
				// ReSharper disable once LoopVariableIsNeverChangedInsideLoop
			} while (Data.SkipGears);

			if (gear == 0) {
				throw new VectoSimulationException("Could not find gear! outTorque: {0}, outEngineSpeed: {1}, skipGears: {2}",
					outTorque, outEngineSpeed, Data.SkipGears);
			}
			return gear;
		}

		public override uint Engage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			return GetGear(LastGear, outTorque, outEngineSpeed);
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			LastGear = GetGear(Gearbox.Gear, outTorque, outEngineSpeed);
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			return ShouldShiftDown(gear, torque, angularSpeed) || ShouldShiftUp(gear, torque, angularSpeed);
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			return Engage(torque, angularSpeed);
		}
	}

	//TODO Implementd MTShiftStrategy
	public class MTShiftStrategy : ShiftStrategy
	{
		public MTShiftStrategy(GearboxData data) : base(data) {}

		public override uint Engage(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}
	}

	// TODO Implement ATShiftStrategy
	public class ATShiftStrategy : ShiftStrategy
	{
		public ATShiftStrategy(GearboxData data) : base(data) {}

		public override uint Engage(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}
	}
}
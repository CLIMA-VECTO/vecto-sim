using System.Diagnostics;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
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
		/// True if gearbox is disengaged (no gear is set).
		/// </summary>
		private bool _disengaged = true;

		/// <summary>
		/// The power loss for the mod data.
		/// </summary>
		private Watt _powerLoss;

		/// <summary>
		/// The inertia power loss for the mod data.
		/// </summary>
		private Watt _powerLossInertia;

		/// <summary>
		/// The previous enginespeed for inertia calculation
		/// </summary>
		private PerSecond _previousInEnginespeed = 0.SI<PerSecond>();


		public bool ClutchClosed(Second absTime)
		{
			return _shiftTime.IsSmaller(absTime);
		}

		public Gearbox(IVehicleContainer container, GearboxData gearboxData, IShiftStrategy strategy = null) : base(container)
		{
			// TODO: do not set a default strategy! gearbox should be called with explicit shift strategy! this is just for debug
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
			_disengaged = false;
			return NextComponent.Initialize(torque, engineSpeed);
		}

		/// <summary>
		/// Requests the Gearbox to deliver outTorque and outEngineSpeed
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item><description>ResponseDryRun</description></item>
		/// <item><description>ResponseOverload</description></item>
		/// <item><description>ResponseGearshift</description></item>
		/// </list>
		/// </returns>
		public IResponse Request(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool dryRun)
		{
			var retVal = ClutchClosed(absTime)
				? RequestGearEngaged(absTime, dt, outTorque, outEngineSpeed, dryRun)
				: RequestGearDisengaged(absTime, dt, outTorque, outEngineSpeed, dryRun);

			return retVal;
		}

		/// <summary>
		/// Requests the Gearbox in Disengaged mode
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item><term>ResponseDryRun</term><description>if dryRun, immediate return!</description></item>
		/// <item><term>ResponseFailTimeInterval</term><description>if shiftTime would be exceeded by current step</description></item>
		/// <item><term>ResponseOverload</term><description>if outTorque &gt; 0</description></item>
		/// <item><term>ResponseUnderload</term><description>if outTorque &lt; 0</description></item>
		/// <item><term>else</term><description>Response from NextComponent</description></item>
		/// </list>
		/// </returns>
		private IResponse RequestGearDisengaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed,
			bool dryRun)
		{
			Log.Debug("Current Gear: Neutral");

			if (dryRun) {
				return new ResponseDryRun {
					Source = this,
					GearboxPowerRequest = outTorque * outEngineSpeed
				};
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
				Gear = _strategy.Engage(outTorque, outEngineSpeed, Data.SkipGears);
			}

			var response = NextComponent.Request(absTime, dt, 0.SI<NewtonMeter>(), null);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;

			return response;
		}

		/// <summary>
		/// Requests the gearbox in engaged mode. Sets the gear if no gear was set previously.
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item><term>ResponseGearShift</term><description>if a shift is needed.</description></item>
		/// <item><term>else</term><description>Response from NextComponent.</description></item>
		/// </list>
		/// </returns>
		private IResponse RequestGearEngaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed,
			bool dryRun)
		{
			// Set a Gear if no gear was set
			if (_disengaged) {
				Gear = _strategy.Engage(outTorque, outEngineSpeed, Data.SkipGears);
				_disengaged = false;
				Log.Debug("Gearbox engaged gear {0}", Gear);
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;
			var torqueLossInertia = !inEngineSpeed.IsEqual(0)
				? Formulas.InertiaPower(inEngineSpeed, _previousInEnginespeed, Data.Inertia, dt) / inEngineSpeed
				: 0.SI<NewtonMeter>();

			_previousInEnginespeed = inEngineSpeed;

			inTorque += torqueLossInertia;

			_powerLossInertia = torqueLossInertia * inEngineSpeed;

			if (dryRun) {
				var dryRunResponse = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed, true);
				dryRunResponse.GearboxPowerRequest = outTorque * outEngineSpeed;
				return dryRunResponse;
			}

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

			var response = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed);
			response.GearboxPowerRequest = outTorque * outEngineSpeed;
			return response;
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
			writer[ModalResultField.PaGB] = _powerLossInertia;
		}

		protected override void DoCommitSimulationStep()
		{
			_powerLoss = null;
			_powerLossInertia = null;
		}

		#endregion
	}
}
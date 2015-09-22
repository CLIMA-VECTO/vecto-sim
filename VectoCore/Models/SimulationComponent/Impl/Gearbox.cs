using System.Diagnostics;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
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
		/// The shift strategy.
		/// </summary>
		private readonly IShiftStrategy _strategy;

		/// <summary>
		/// Time when a gearbox shift engages a new gear (shift is finished). Is set when shifting is needed.
		/// </summary>
		private Second _shiftTime = 0.SI<Second>();

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
			return _shiftTime.IsSmallerOrEqual(absTime);
		}

		public Gearbox(IVehicleContainer container, GearboxData gearboxData, IShiftStrategy strategy) : base(container)
		{
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

		[DebuggerHidden]
		public MeterPerSecond StartSpeed
		{
			get { return Data.StartSpeed; }
		}

		[DebuggerHidden]
		public MeterPerSquareSecond StartAcceleration
		{
			get { return Data.StartAcceleration; }
		}

		#endregion

		#region ITnOutPort

		public IResponse Initialize(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;
			_shiftTime = double.NegativeInfinity.SI<Second>();

			if (_disengaged) {
				Gear = _strategy.InitGear(absTime, dt, outTorque, outEngineSpeed);
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;
			var torqueLossInertia = inEngineSpeed.IsEqual(0)
				? 0.SI<NewtonMeter>()
				: Formulas.InertiaPower(inEngineSpeed, _previousInEnginespeed, Data.Inertia, dt) / inEngineSpeed;

			inTorque += torqueLossInertia;
			var response = NextComponent.Initialize(inTorque, inEngineSpeed);
			if (response is ResponseSuccess) {
				_previousInEnginespeed = inEngineSpeed;
				_disengaged = false;
			}

			return response;
		}


		internal ResponseDryRun Initialize(uint gear, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			var inEngineSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
			var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			_powerLoss = inTorque * inEngineSpeed - outTorque * outEngineSpeed;

			PerSquareSecond alpha;
			if (Data.Inertia.IsEqual(0)) {
				alpha = 0.SI<PerSquareSecond>();
			} else {
				alpha = outTorque / Data.Inertia;
			}

			var torqueLossInertia = inEngineSpeed.IsEqual(0)
				? 0.SI<NewtonMeter>()
				: Formulas.InertiaPower(inEngineSpeed, alpha, Data.Inertia, Constants.SimulationSettings.TargetTimeInterval) /
				inEngineSpeed;

			inTorque += torqueLossInertia;
			var response = NextComponent.Initialize(inTorque, inEngineSpeed);

			response.Switch().
				Case<ResponseSuccess>().
				Case<ResponseOverload>().
				Case<ResponseUnderload>().
				Default(r => {
					throw new UnexpectedResponseException("Gearbox.Initialize", r);
				});

			var fullLoadGearbox = Data.Gears[gear].FullLoadCurve.FullLoadStationaryTorque(inEngineSpeed) * inEngineSpeed;
			var fullLoadEngine = DataBus.EngineStationaryFullPower(inEngineSpeed);
			return new ResponseDryRun {
				Source = this,
				EnginePowerRequest = response.EnginePowerRequest,
				DeltaFullLoad = response.EnginePowerRequest - VectoMath.Min(fullLoadGearbox, fullLoadEngine)
			};
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
			if (DataBus.VehicleStopped) {
				_shiftTime = absTime;
			}
			IResponse retVal;
			if (ClutchClosed(absTime)) {
				retVal = RequestGearEngaged(absTime, dt, outTorque, outEngineSpeed, dryRun);
			} else {
				retVal = RequestGearDisengaged(absTime, dt, outTorque, outEngineSpeed, dryRun);
			}

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

			//if (DataBus.VehicleSpeed.IsEqual(0)) {
			//	Gear = _strategy.Engage(outTorque, outEngineSpeed, Data.SkipGears);
			//}

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
			// Set a Gear if no gear was set and engineSpeed is not zero
			if (_disengaged && !outEngineSpeed.IsEqual(0)) {
				_disengaged = false;
				if (DataBus.VehicleSpeed.IsEqual(0)) {
					Gear = _strategy.InitGear(absTime, dt, outTorque, outEngineSpeed);
				} else {
					Gear = _strategy.Engage(absTime, dt, outTorque, outEngineSpeed);
				}

				Log.Debug("Gearbox engaged gear {0}", Gear);
			}

			var inEngineSpeed = outEngineSpeed * Data.Gears[Gear].Ratio;
			NewtonMeter inTorque;

			if (outEngineSpeed.IsEqual(0)) {
				inTorque = outTorque / Data.Gears[Gear].Ratio;
			} else {
				inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);
			}
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

			if (!outEngineSpeed.IsEqual(0)) {
				var isShiftAllowed = (_shiftTime + Data.ShiftTime).IsSmallerOrEqual(absTime);
				if (isShiftAllowed && _strategy.ShiftRequired(Gear, inTorque, inEngineSpeed)) {
					_shiftTime = absTime + Data.TractionInterruption;

					Log.Debug("Gearbox is shifting. absTime: {0}, dt: {1}, shiftTime: {2}, out: ({3}, {4}), in: ({5}, {6})", absTime,
						dt,
						_shiftTime, outTorque, outEngineSpeed, inTorque, inEngineSpeed);

					_disengaged = true;
					_strategy.Disengage(absTime, dt, outTorque, outEngineSpeed);
					Log.Info("Gearbox disengaged");

					return new ResponseGearShift {
						Source = this,
						SimulationInterval = Data.TractionInterruption,
						GearboxPowerRequest = outTorque * outEngineSpeed
					};
				}
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
			writer[ModalResultField.Gear] = _disengaged ? 0 : Gear;
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
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
		private PerSecond _previousInAngularSpeed = 0.SI<PerSecond>();

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

		public FullLoadCurve GearFullLoadCurve
		{
			get { return Gear == 0 ? null : Data.Gears[Gear].FullLoadCurve; }
		}

		#endregion

		#region ITnOutPort

		public IResponse Initialize(NewtonMeter outTorque, PerSecond outAngularVelocity)
		{
			var absTime = 0.SI<Second>();
			var dt = Constants.SimulationSettings.TargetTimeInterval;
			_shiftTime = double.NegativeInfinity.SI<Second>();
			_powerLoss = null;

			if (_disengaged) {
				Gear = _strategy.InitGear(absTime, dt, outTorque, outAngularVelocity);
			}

			var inAngularVelocity = outAngularVelocity * Data.Gears[Gear].Ratio;
			var inTorque = Data.Gears[Gear].LossMap.GearboxInTorque(inAngularVelocity, outTorque);

			var torqueLossInertia = outAngularVelocity.IsEqual(0)
				? 0.SI<NewtonMeter>()
				: Formulas.InertiaPower(inAngularVelocity, _previousInAngularSpeed, Data.Inertia, dt) / inAngularVelocity;

			inTorque += torqueLossInertia;

			var response = NextComponent.Initialize(inTorque, inAngularVelocity);
			if (response is ResponseSuccess) {
				_previousInAngularSpeed = inAngularVelocity;
				_disengaged = false;
			}

			return response;
		}

		internal ResponseDryRun Initialize(uint gear, NewtonMeter outTorque, PerSecond outAngularVelocity)
		{
			var inAngularVelocity = outAngularVelocity * Data.Gears[gear].Ratio;
			var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inAngularVelocity, outTorque);

			if (!inAngularVelocity.IsEqual(0)) {
				var alpha = (Data.Inertia.IsEqual(0))
					? 0.SI<PerSquareSecond>()
					: outTorque / Data.Inertia;

				var inertiaPowerLoss = Formulas.InertiaPower(inAngularVelocity, alpha, Data.Inertia,
					Constants.SimulationSettings.TargetTimeInterval);
				inTorque += inertiaPowerLoss / inAngularVelocity;
			}

			var response = NextComponent.Initialize(inTorque, inAngularVelocity);
			response.Switch().
				Case<ResponseSuccess>().
				Case<ResponseOverload>().
				Case<ResponseUnderload>().
				Default(r => {
					throw new UnexpectedResponseException("Gearbox.Initialize", r);
				});

			var fullLoadGearbox = Data.Gears[gear].FullLoadCurve.FullLoadStationaryTorque(inAngularVelocity) * inAngularVelocity;
			var fullLoadEngine = DataBus.EngineStationaryFullPower(inAngularVelocity);

			var fullLoad = VectoMath.Min(fullLoadGearbox, fullLoadEngine);

			return new ResponseDryRun {
				Source = this,
				EnginePowerRequest = response.EnginePowerRequest,
				ClutchPowerRequest = response.ClutchPowerRequest,
				GearboxPowerRequest = outTorque * outAngularVelocity,
				DeltaFullLoad = response.EnginePowerRequest - fullLoad
			};
		}

		/// <summary>
		/// Requests the Gearbox to deliver torque and angularVelocity
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item><description>ResponseDryRun</description></item>
		/// <item><description>ResponseOverload</description></item>
		/// <item><description>ResponseGearshift</description></item>
		/// </list>
		/// </returns>
		public IResponse Request(Second absTime, Second dt, NewtonMeter torque, PerSecond angularVelocity, bool dryRun)
		{
			Log.Debug("Gearbox Power Request: torque: {0}, angularVelocity: {1}", torque, angularVelocity);
			if (DataBus.VehicleStopped) {
				_shiftTime = absTime;
			}
			IResponse retVal;
			if (ClutchClosed(absTime)) {
				retVal = RequestGearEngaged(absTime, dt, torque, angularVelocity, dryRun);
			} else {
				retVal = RequestGearDisengaged(absTime, dt, torque, angularVelocity, dryRun);
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
		/// <item><term>ResponseOverload</term><description>if torque &gt; 0</description></item>
		/// <item><term>ResponseUnderload</term><description>if torque &lt; 0</description></item>
		/// <item><term>else</term><description>Response from NextComponent</description></item>
		/// </list>
		/// </returns>
		private IResponse RequestGearDisengaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			bool dryRun)
		{
			Log.Debug("Current Gear: Neutral");

			if (dryRun) {
				return new ResponseDryRun {
					Source = this,
					GearboxPowerRequest = outTorque * outAngularVelocity
				};
			}

			var shiftTimeExceeded = absTime.IsSmaller(_shiftTime) &&
									_shiftTime.IsSmaller(absTime + dt, Data.TractionInterruption / 20.0);
			if (shiftTimeExceeded) {
				return new ResponseFailTimeInterval {
					Source = this,
					DeltaT = _shiftTime - absTime,
					GearboxPowerRequest = outTorque * outAngularVelocity
				};
			}

			if ((outTorque * outAngularVelocity).IsGreater(0.SI<Watt>(), Constants.SimulationSettings.EnginePowerSearchTolerance)) {
				return new ResponseOverload {
					Source = this,
					Delta = outTorque * outAngularVelocity,
					GearboxPowerRequest = outTorque * outAngularVelocity
				};
			}

			if ((outTorque * outAngularVelocity).IsSmaller(0.SI<Watt>(), Constants.SimulationSettings.EnginePowerSearchTolerance)) {
				return new ResponseUnderload {
					Source = this,
					Delta = outTorque * outAngularVelocity,
					GearboxPowerRequest = outTorque * outAngularVelocity
				};
			}

			var response = NextComponent.Request(absTime, dt, 0.SI<NewtonMeter>(), null);
			response.GearboxPowerRequest = outTorque * outAngularVelocity;

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
		private IResponse RequestGearEngaged(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			bool dryRun)
		{
			// Set a Gear if no gear was set and engineSpeed is not zero
			if (_disengaged && !outAngularVelocity.IsEqual(0)) {
				_disengaged = false;
				if (DataBus.VehicleStopped) {
					Gear = _strategy.InitGear(absTime, dt, outTorque, outAngularVelocity);
				} else {
					Gear = _strategy.Engage(absTime, dt, outTorque, outAngularVelocity);
				}

				Log.Debug("Gearbox engaged gear {0}", Gear);
			}

			var inEngineSpeed = outAngularVelocity * Data.Gears[Gear].Ratio;
			var inTorque = (outAngularVelocity.IsEqual(0))
				? outTorque / Data.Gears[Gear].Ratio
				: Data.Gears[Gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);

			_powerLoss = inTorque * inEngineSpeed - outTorque * outAngularVelocity;

			if (!inEngineSpeed.IsEqual(0)) {
				_powerLossInertia = Formulas.InertiaPower(inEngineSpeed, _previousInAngularSpeed, Data.Inertia, dt);
				inTorque += _powerLossInertia / inEngineSpeed;
			} else {
				_powerLossInertia = 0.SI<Watt>();
			}

			if (dryRun) {
				if ((DataBus.DrivingBehavior == DrivingBehavior.Braking || DataBus.DrivingBehavior == DrivingBehavior.Coasting) &&
					inEngineSpeed < DataBus.EngineIdleSpeed &&
					DataBus.VehicleSpeed < Constants.SimulationSettings.VehicleStopClutchDisengageSpeed) {
					_disengaged = true;
					_shiftTime = absTime + dt;
					_strategy.Disengage(absTime, dt, outTorque, outAngularVelocity);
					Log.Debug("EngineSpeed is below IdleSpeed, Gearbox disengage!");
					return new ResponseEngineSpeedTooLow() { Source = this, GearboxPowerRequest = outTorque * outAngularVelocity };
				}
				var dryRunResponse = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed, true);
				dryRunResponse.GearboxPowerRequest = outTorque * outAngularVelocity;
				return dryRunResponse;
			}

			var shiftAllowed = !inEngineSpeed.IsEqual(0) && !DataBus.VehicleSpeed.IsEqual(0);

			if (shiftAllowed) {
				var shiftRequired = _strategy.ShiftRequired(absTime, dt, outTorque, outAngularVelocity, inTorque, inEngineSpeed,
					Gear, _shiftTime + Data.TractionInterruption);

				if (shiftRequired) {
					_shiftTime = absTime + Data.TractionInterruption;

					Log.Debug("Gearbox is shifting. absTime: {0}, dt: {1}, shiftTime: {2}, out: ({3}, {4}), in: ({5}, {6})", absTime,
						dt, _shiftTime, outTorque, outAngularVelocity, inTorque, inEngineSpeed);

					_disengaged = true;
					_strategy.Disengage(absTime, dt, outTorque, outAngularVelocity);
					Log.Info("Gearbox disengaged");

					return new ResponseGearShift {
						Source = this,
						SimulationInterval = Data.TractionInterruption,
						GearboxPowerRequest = outTorque * outAngularVelocity
					};
				}
			}

			var response = NextComponent.Request(absTime, dt, inTorque, inEngineSpeed);
			response.GearboxPowerRequest = outTorque * outAngularVelocity;

			_previousInAngularSpeed = inEngineSpeed;
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
			writer[ModalResultField.Gear] = _disengaged || DataBus.VehicleStopped ? 0 : Gear;
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
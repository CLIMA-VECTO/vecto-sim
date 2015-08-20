using System.Collections.Generic;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class EngineOnlyCombustionEngine : CombustionEngine
	{
		protected readonly List<Second> _enginePowerCorrections = new List<Second>();

		public EngineOnlyCombustionEngine(IVehicleContainer cockpit, CombustionEngineData data) : base(cockpit, data) {}

		// the behavior in engin-only mode differs a little bit from normal driving cycle simulation: in engine-only mode
		// certain amount of overload is tolerated.
		protected override IResponse DoHandleRequest(Second absTime, Second dt, NewtonMeter torque, PerSecond engineSpeed,
			bool dryRun)
		{
			Watt requestedPower;
			Watt requestedEnginePower;
			ComputeRequestedEnginePower(absTime, dt, torque, engineSpeed, out requestedPower, out requestedEnginePower);

			ComputeFullLoadPower(engineSpeed, dt);

			ValidatePowerDemand(requestedEnginePower);

			_currentState.EnginePower = LimitEnginePower(requestedEnginePower);

			if (dryRun) {
				return new ResponseDryRun {
					DeltaFullLoad = (requestedEnginePower - _currentState.DynamicFullLoadPower),
					DeltaDragLoad = (requestedEnginePower - _currentState.FullDragPower)
				};
			}

			UpdateEngineState(_currentState.EnginePower);

			// = requestedEnginePower; //todo + _currentState.EnginePowerLoss;
			_currentState.EngineTorque = Formulas.PowerToTorque(_currentState.EnginePower,
				_currentState.EngineSpeed);

			return new ResponseSuccess();
		}

		/// <summary>
		///     [W] => [W]
		/// </summary>
		/// <param name="requestedEnginePower">[W]</param>
		/// <returns>[W]</returns>
		protected override Watt LimitEnginePower(Watt requestedEnginePower)
		{
			if (requestedEnginePower > _currentState.DynamicFullLoadPower) {
				if (requestedEnginePower / _currentState.DynamicFullLoadPower > MaxPowerExceededThreshold) {
					_enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat(
						"t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}",
						_currentState.AbsTime, requestedEnginePower, _currentState.DynamicFullLoadPower);
				}
				return _currentState.DynamicFullLoadPower;
			}
			if (requestedEnginePower < _currentState.FullDragPower) {
				if (requestedEnginePower / _currentState.FullDragPower > MaxPowerExceededThreshold &&
					requestedEnginePower > -99999) {
					_enginePowerCorrections.Add(_currentState.AbsTime);
					Log.WarnFormat(
						"t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}",
						_currentState.AbsTime, requestedEnginePower, _currentState.FullDragPower);
				}
				return _currentState.FullDragPower;
			}
			return requestedEnginePower;
		}

		public IList<string> Warnings()
		{
			IList<string> retVal = new List<string>();
			retVal.Add(string.Format("Engine power corrected (>5%) in {0} time steps ", _enginePowerCorrections.Count));
			return retVal;
		}
	}
}
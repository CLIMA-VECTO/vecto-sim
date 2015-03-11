using System;
using System.Collections.Generic;
using NLog;
using NLog.Layouts;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{

	public class CombustionEngine : VectoSimulationComponent, ICombustionEngine, ITnOutPort
	{
		private const int EngineIdleSpeedStopThreshold = 100;

		public enum EngineOperationMode
		{
			Idle,
			Drag,
			FullDrag,
			Load,
			FullLoad,
			Stopped,
			Undef
		}

		private CombustionEngineData _data;

		private EngineState _previousState = new EngineState();
		private EngineState _currentState = new EngineState();	// current state is computed in request method

		private List<TimeSpan> _enginePowerCorrections = new List<TimeSpan>(); 

		public CombustionEngine(CombustionEngineData data)
		{
			_data = data;

			_previousState.OperationMode = EngineOperationMode.Idle;
			_previousState.EnginePower = 0;
			_previousState.EngineSpeed = _data.IdleSpeed;
		}


        public ITnOutPort OutShaft()
		{
			return this;
		}

	    public override void CommitSimulationStep(IModalDataWriter writer)
	    {
	        writer[ModalResultField.FC] = 1;
	        writer[ModalResultField.FCAUXc] = 2;
            writer[ModalResultField.FCWHTCc] = 3;
	    }

        public void Request(TimeSpan absTime, TimeSpan dt, double torque, double engineSpeed)
        {
			var requestedPower = VectoMath.ConvertRpmToPower(engineSpeed, torque);
			var enginePowerLoss = InertiaPowerLoss(torque, engineSpeed);
			var requestedEnginePower = requestedPower + enginePowerLoss;

			if (engineSpeed < _data.IdleSpeed - EngineIdleSpeedStopThreshold) {
		        _currentState.OperationMode = EngineOperationMode.Stopped;
				_currentState.EnginePowerLoss = enginePowerLoss
	        }
	
			uint currentGear = 0; // TODO: get current Gear from Vehicle!

	        var minEnginePower = _data.GetFullLoadCurve(currentGear).DragLoadStaticPower(engineSpeed);
	        var maxEnginePower = FullLoadPowerDyamic(currentGear, engineSpeed);

	        if (minEnginePower >= 0 && requestedPower < 0) {
		        throw new VectoSimulationException(String.Format("t: {0}  P_engine_drag > 0! n: {1} [1/min] ", absTime, engineSpeed));
	        }
	        if (maxEnginePower <= 0 && requestedPower > 0) {
		        throw new VectoSimulationException(String.Format("t: {0}  P_engine_full < 0! n: {1} [1/min] ", absTime, engineSpeed)));
	        }

	        if (requestedPower > maxEnginePower) {
		        if (requestedEnginePower/maxEnginePower > 1.05) {
			        requestedEnginePower = maxEnginePower;
					_enginePowerCorrections.Add(absTime);
					Log.WarnFormat("t: {0}  requested power > P_engine_full * 1.05 - corrected. P_request: {1}  P_engine_full: {2}", absTime, requestedEnginePower, maxEnginePower);
		        }
	        } else if (requestedPower < minEnginePower) {
		        if (requestedEnginePower/minEnginePower > 1.05 && requestedEnginePower > -99999) {
			        requestedEnginePower = minEnginePower;
					_enginePowerCorrections.Add(absTime);
					Log.WarnFormat("t: {0}  requested power < P_engine_drag * 1.05 - corrected. P_request: {1}  P_engine_drag: {2}", absTime, requestedEnginePower, minEnginePower);
		        }
	        }

	        //throw new NotImplementedException();
        }

		protected double FullLoadPowerDyamic(uint gear, double rpm)
		{
			var staticFullLoadPower = _data.GetFullLoadCurve(gear).FullLoadStaticPower(rpm);
			var pt1 = _data.GetFullLoadCurve(gear).PT1(rpm);

			return Math.Min( (1 / (pt1 + 1)) * (staticFullLoadPower + pt1 * _previousState.EnginePower),
				staticFullLoadPower);
		}


		protected double InertiaPowerLoss(double torque, double engineSpeed)
		{
			var deltaEngineSpeed = engineSpeed - _previousState.EngineSpeed;
			var avgEngineSpeed = (_previousState.EngineSpeed + engineSpeed) / 2.0;
			return _data.Inertia*VectoMath.RpmTpAngularVelocity(deltaEngineSpeed)*VectoMath.RpmTpAngularVelocity(avgEngineSpeed);
		}

		// accelleration los rotation engine
		//Return (ENG.I_mot * (nU - nUBefore) * 0.01096 * ((nU + nUBefore) / 2)) * 0.001

			public class EngineState
			{
				public EngineOperationMode OperationMode { get; set; }
				public double EnginePower { get; set; }
				public double EngineSpeed { get; set; }
				public double EnginePowerLoss { get; set; }
			}
	}


}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class VehicleContainer : IVehicleContainer
	{
		private readonly IList<VectoSimulationComponent> _components = new List<VectoSimulationComponent>();
		private IEngineCockpit _engine;
		private IGearboxCockpit _gearbox;
		private ILog _logger;
		private ISummaryDataWriter _sumWriter;
		private IModalDataWriter _dataWriter;
		private string _jobName;
		private string _cycleFileName;
		private string _jobFileName;

		#region IGearCockpit

		public uint Gear()
		{
			if (_gearbox == null) {
				throw new VectoException("no gearbox available!");
			}
			return _gearbox.Gear();
		}

		#endregion

		#region IEngineCockpit

		public PerSecond EngineSpeed()
		{
			if (_engine == null) {
				throw new VectoException("no engine available!");
			}
			return _engine.EngineSpeed();
		}

		#endregion

		#region IVehicleCockpit

		public MeterPerSecond VehicleSpeed()
		{
			throw new VectoException("no vehicle available!");
		}

		#endregion

		public VehicleContainer(IModalDataWriter dataWriter = null, ISummaryDataWriter sumWriter = null,
			string jobFileName = "",
			string jobName = "",
			string cycleFileName = "")
		{
			_logger = LogManager.GetLogger(GetType());
			_dataWriter = dataWriter;
			_sumWriter = sumWriter;
			_jobFileName = jobFileName;
			_jobName = jobName;
			_cycleFileName = cycleFileName;
		}

		#region IVehicleContainer

		public virtual void AddComponent(VectoSimulationComponent component)
		{
			_components.Add(component);

			// TODO: refactor the following to use polymorphism?
			var engine = component as IEngineCockpit;
			if (engine != null) {
				_engine = engine;
			}

			var gearbox = component as IGearboxCockpit;
			if (gearbox != null) {
				_gearbox = gearbox;
			}
		}


		public void CommitSimulationStep(double time, double simulationInterval)
		{
			_logger.Info("VehicleContainer committing simulation.");
			foreach (var component in _components) {
				component.CommitSimulationStep(_dataWriter);
			}

			_dataWriter[ModalResultField.time] = time;
			_dataWriter[ModalResultField.simulationInterval] = simulationInterval;
			_dataWriter.CommitSimulationStep();
		}

		public void FinishSimulation()
		{
			_logger.Info("VehicleContainer finishing simulation.");
			_dataWriter.Finish();

			_sumWriter.Write(_dataWriter, _jobFileName, _jobName, _cycleFileName);
		}

		#endregion

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(_components);
		}
	}
}
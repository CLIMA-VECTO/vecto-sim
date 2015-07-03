using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
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
		private IVehicleCockpit _vehicle;

		private IDrivingCycleOutPort _cycle;

		private ISummaryDataWriter _sumWriter;
		private IModalDataWriter _dataWriter;

		private ILog _logger;

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
			return _vehicle != null ? _vehicle.VehicleSpeed() : 0.SI<MeterPerSecond>();
		}

		public Kilogram VehicleMass()
		{
			return _vehicle != null ? _vehicle.VehicleMass() : 0.SI<Kilogram>();
		}

		public Kilogram VehicleLoading()
		{
			return _vehicle != null ? _vehicle.VehicleLoading() : 0.SI<Kilogram>();
		}

		public Kilogram TotalMass()
		{
			return _vehicle != null ? _vehicle.TotalMass() : 0.SI<Kilogram>();
		}

		#endregion

		public VehicleContainer(IModalDataWriter dataWriter = null, ISummaryDataWriter sumWriter = null)
		{
			_logger = LogManager.GetLogger(GetType());
			_dataWriter = dataWriter;
			_sumWriter = sumWriter;
		}

		#region IVehicleContainer

		public IDrivingCycleOutPort GetCycleOutPort()
		{
			return _cycle;
		}

		public virtual void AddComponent(VectoSimulationComponent component)
		{
			_components.Add(component);

			var engine = component as IEngineCockpit;
			if (engine != null) {
				_engine = engine;
			}

			var gearbox = component as IGearboxCockpit;
			if (gearbox != null) {
				_gearbox = gearbox;
			}

			var vehicle = component as IVehicleCockpit;
			if (vehicle != null) {
				_vehicle = vehicle;
			}

			var cycle = component as IDrivingCycleOutPort;
			if (cycle != null) {
				_cycle = cycle;
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

			_sumWriter.Write(_dataWriter, VehicleMass(), VehicleLoading());
		}

		#endregion

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(_components);
		}
	}
}
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


		public void CommitSimulationStep(IModalDataWriter dataWriter)
		{
			LogManager.GetLogger(GetType()).Info("VehicleContainer committing simulation.");
			foreach (var component in _components) {
				component.CommitSimulationStep(dataWriter);
			}
			dataWriter.CommitSimulationStep();
		}

		public void FinishSimulation(IModalDataWriter dataWriter)
		{
			LogManager.GetLogger(GetType()).Info("VehicleContainer finishing simulation.");
			dataWriter.Finish();
		}

		#endregion

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(_components);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation
{
	public class SimulationContainer
	{
		private IList<VectoSimulationComponent> _components = new List<VectoSimulationComponent>();

		private ICombustionEngine _engine = null;
		private IGearbox _gearbox = null;

		public void AddComponent(VectoSimulationComponent component)
		{
			DoAddComponent(component);
		}

		public void Addcomponent<T>(T engine) where T : VectoSimulationComponent, ICombustionEngine
		{
			_engine = engine;
			DoAddComponent(engine);
		}

		public void AddComponent<T>(T gearbox) where T : VectoSimulationComponent, IGearbox
		{
			_gearbox = gearbox;
			DoAddComponent(gearbox);
		}

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(_components);
		}

		public ICombustionEngine CombustionEngine()
		{
			return _engine;
		}
	
		public IGearbox Gearbox()
		{
			return _gearbox;
		}



		protected void DoAddComponent(VectoSimulationComponent component)
		{
			_components.Add(component);
		}


	}
}

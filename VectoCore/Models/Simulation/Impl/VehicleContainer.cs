using System.Collections.Generic;
using System.Collections.ObjectModel;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Cockpit;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{


	public class VehicleContainer : IVehicleContainer
	{
		private readonly IList<VectoSimulationComponent> _components = new List<VectoSimulationComponent>();

		private IEngineCockpit _engine = null;
		private IGearboxCockpit _gearbox = null;

		public virtual void AddComponent(VectoSimulationComponent component)
		{
			DoAddComponent(component);
			// TODO: refactor the following to use polymorphism?
			if (component is IEngineCockpit) {
				_engine = (IEngineCockpit)component;
			}
			else if (component is IGearboxCockpit) {
				_gearbox = (IGearboxCockpit) component;
			}
		}


		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(_components);
		}

		public uint Gear()
		{
			if (_gearbox == null) 
				throw new VectoException("no gearbox available!");
			return _gearbox.Gear();
		}

		public double EngineSpeed()
		{
			if (_engine == null)
				throw new VectoException("no engine available!");
			return _engine.EngineSpeed();
		}

		public double VehicleSpeed()
		{
			throw new VectoException("no vehicle available!");
		}

		protected void DoAddComponent(VectoSimulationComponent component)
		{
			_components.Add(component);
		}


        public ITnOutPort GetEngineOnlyStartPort()
	    {
            //todo: find a better solution for getting the initial port in a powertrain
	        return (_gearbox as IGearbox).OutShaft();
	    }

	    public void CommitSimulationStep(IModalDataWriter dataWriter)
	    {
            foreach (var c in _components)
            {
                c.CommitSimulationStep(dataWriter);
            }
	    }

        public void FinishSimulation(IModalDataWriter dataWriter)
        {
            dataWriter.Finish();
        }
	}
}

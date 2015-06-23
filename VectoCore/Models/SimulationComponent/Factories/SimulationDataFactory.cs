using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Factories.Impl;

namespace TUGraz.VectoCore.Models.SimulationComponent.Factories
{
	//public class SimulationDataFactory : ISimulationDataFactory
	//{
	//	private IDataFileReader _concreteFactory;
	//	private readonly Mode _mode;

	//	public enum Mode
	//	{
	//		DeclarationMode,
	//		EngineeringMode
	//	}

	//	public SimulationDataFactory(Mode mode)
	//	{
	//		_mode = mode;
	//		switch (mode) {
	//			case Mode.DeclarationMode:
	//				_concreteFactory = new DeclarationModeSimulationComponentFactory();
	//				break;
	//			case Mode.EngineeringMode:
	//				_concreteFactory = new EngineeringModeSimulationComponentFactory();
	//				break;
	//		}
	//	}


	//	public void SetJobFile(string fileName)
	//	{
	//		throw new System.NotImplementedException();
	//	}

	//	public void SetJobJson(string jsonData, string basePath)
	//	{
	//		throw new System.NotImplementedException();
	//	}

	//	public Mode FactoryMode()
	//	{
	//		return _mode;
	//	}
	//}
}
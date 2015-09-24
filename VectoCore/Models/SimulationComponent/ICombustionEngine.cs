﻿using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.DataBus;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Defines Interfaces for a combustion engine.
	/// </summary>
	public interface ICombustionEngine : ITnOutProvider, IEngineInfo
	{
		ICombustionEngineIdleController GetIdleController();
	}
}
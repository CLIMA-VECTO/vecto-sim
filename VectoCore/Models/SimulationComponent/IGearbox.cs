﻿using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Cockpit;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Defines interfaces for a gearbox.
	/// </summary>
	public interface IGearbox : IInShaft, IOutShaft, IGearboxCockpit {}
}
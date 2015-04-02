using System;
using TUGraz.VectoCore.Models.Connector.Ports;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IDrivingCycle
	{
		IResponse Request(TimeSpan absTime, TimeSpan dt);
	}

	public interface IDriverDemandDrivingCycle : IDrivingCycle, IDriverDemandInProvider {}

	public interface IEngineOnlyDrivingCycle : IDrivingCycle, IInShaft {}
}
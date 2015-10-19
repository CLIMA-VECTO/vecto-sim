﻿using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.DataBus
{
	/// <summary>
	/// Defines interfaces for all different cockpits to access shared data of the powertrain.
	/// </summary>
	public interface IDataBus : IGearboxInfo, IEngineInfo, IVehicleInfo, IMileageCounter, IClutchInfo, IBrakes,
		IRoadLookAhead, IDriverInfo {}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class CombustionEngineData : SimulationComponentData
	{
		/// <summary>
		/// Engine description (e.g., mode, type, etc.
		/// </summary>
		public String ModelName { get; protected set; }
		
		/// <summary>
		/// Engine displacement [ccm]
		/// </summary>
		public double Displacement { get; protected set; }

		public double IdleSpeed { get; protected set; }

		public double RatedSpeed { get; protected set; }

		public double Inertia { get; protected set; }

		public double MaxPower { get; set; }

		public FuelConsumptionMap ConsumptionMap { get; protected set; }

		public FullLoadCurve FullLoadCurve(uint gear)
		{
			throw new NotImplementedException("get FullLoadCurve for gear");
		}
	}
}

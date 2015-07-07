using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Impl;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	/// <summary>
	/// Provides Methods to build a simulator with a powertrain step by step.
	/// </summary>
	public class PowertrainBuilder
	{
		private readonly bool _engineOnly;
		private readonly VehicleContainer _container;


		public PowertrainBuilder(IModalDataWriter dataWriter, ISummaryDataWriter sumWriter, bool engineOnly)
		{
			_engineOnly = engineOnly;
			_container = new VehicleContainer(dataWriter, sumWriter);
		}

		public VehicleContainer Build(VectoRunData data)
		{
			return _engineOnly ? BuildEngineOnly(data) : BuildFullPowertrain(data);
		}

		private VehicleContainer BuildFullPowertrain(VectoRunData data)
		{
			//todo: make distinction between time based and distance based driving cycle!
			var cycle = new DistanceBasedDrivingCycle(_container, data.Cycle);

			// connect cycle --> driver --> vehicle --> wheels --> axleGear --> gearBox --> retarder --> clutch
			dynamic tmp = AddComponent(cycle, new Driver(_container, data.DriverData));
			tmp = AddComponent(tmp, new Vehicle(_container, data.VehicleData));
			tmp = AddComponent(tmp, new Wheels(_container, data.VehicleData.DynamicTyreRadius));
			tmp = AddComponent(tmp, new AxleGear(_container, data.GearboxData.AxleGearData));

			switch (data.VehicleData.Retarder.Type) {
				case RetarderData.RetarderType.Primary:
					tmp = AddComponent(tmp, new Retarder(_container, data.VehicleData.Retarder.LossMap));
					tmp = AddComponent(tmp, GetGearbox(_container, data.GearboxData));
					break;
				case RetarderData.RetarderType.Secondary:
					tmp = AddComponent(tmp, GetGearbox(_container, data.GearboxData));
					tmp = AddComponent(tmp, new Retarder(_container, data.VehicleData.Retarder.LossMap));
					break;
				case RetarderData.RetarderType.None:
					tmp = AddComponent(tmp, GetGearbox(_container, data.GearboxData));
					break;
			}

			tmp = AddComponent(tmp, new Clutch(_container, data.EngineData));

			// connect cluch --> aux --> ... --> aux_XXX --> directAux
			if (data.Aux != null) {
				foreach (var auxData in data.Aux) {
					var auxCycleData = new AuxiliaryCycleDataAdapter(data.Cycle, auxData.ID);
					IAuxiliary auxiliary = new MappingAuxiliary(_container, auxCycleData, auxData.Data);
					tmp = AddComponent(tmp, auxiliary);
				}
			}

			// connect directAux --> engine
			IAuxiliary directAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(data.Cycle));
			tmp = AddComponent(tmp, directAux);

			AddComponent(tmp, new CombustionEngine(_container, data.EngineData));

			return _container;
		}

		protected IGearbox GetGearbox(VehicleContainer container, GearboxData data)
		{
			switch (data.Type) {
				case GearboxData.GearboxType.AT:
					throw new VectoSimulationException("Unsupported Geabox type: Automatic Transmission (AT)");
				case GearboxData.GearboxType.Custom:
					throw new VectoSimulationException("Custom Gearbox not supported");
				default:
					return new Gearbox(container, data);
			}
		}

		protected virtual IDriver AddComponent(IDrivingCycleDemandDrivingCycle prev, IDriver next)
		{
			prev.InShaft().Connect(next.OutShaft());
			return next;
		}

		protected virtual IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InShaft().Connect(next.OutShaft());
			return next;
		}

		protected virtual IWheels AddComponent(IRoadPortInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		protected virtual IPowerTrainComponent AddComponent(IWheels prev, IPowerTrainComponent next)
		{
			prev.InShaft().Connect(next.OutShaft());
			return next;
		}

		protected virtual IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InShaft().Connect(next.OutShaft());
			return next;
		}

		protected virtual void AddComponent(IPowerTrainComponent prev, IOutShaft next)
		{
			prev.InShaft().Connect(next.OutShaft());
		}


		private VehicleContainer BuildEngineOnly(VectoRunData data)
		{
			var cycle = new EngineOnlyDrivingCycle(_container, data.Cycle);

			var engine = new CombustionEngine(_container, data.EngineData);
			var gearBox = new EngineOnlyGearbox(_container);

			IAuxiliary addAux = new DirectAuxiliary(_container, new AuxiliaryCycleDataAdapter(data.Cycle));
			addAux.InShaft().Connect(engine.OutShaft());

			gearBox.InShaft().Connect(addAux.OutShaft());

			cycle.InShaft().Connect(gearBox.OutShaft());

			return _container;
		}
	}
}
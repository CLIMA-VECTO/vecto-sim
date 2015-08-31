using System.Collections;
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
		private readonly IModalDataWriter _dataWriter;


		public PowertrainBuilder(IModalDataWriter dataWriter, ISummaryDataWriter sumWriter, bool engineOnly)
		{
			_engineOnly = engineOnly;
			_dataWriter = dataWriter;
			_container = new VehicleContainer(dataWriter, sumWriter);
		}

		public VehicleContainer Build(VectoRunData data)
		{
			return _engineOnly ? BuildEngineOnly(data) : BuildFullPowertrain(data);
		}

		private VehicleContainer BuildFullPowertrain(VectoRunData data)
		{
			IDrivingCycle cycle;
			switch (data.Cycle.CycleType) {
				case CycleType.EngineOnly:
					throw new VectoSimulationException("Engine-Only cycle File for full PowerTrain not allowed!");
				case CycleType.DistanceBased:
					cycle = new DistanceBasedDrivingCycle(_container, data.Cycle);
					break;
				case CycleType.TimeBased:
					cycle = new TimeBasedDrivingCycle(_container, data.Cycle);
					break;
			}
			// cycle --> driver --> vehicle --> wheels --> axleGear --> retarder --> gearBox
			var driver = AddComponent(cycle, new Driver(_container, data.DriverData));
			var vehicle = AddComponent(driver, new Vehicle(_container, data.VehicleData));
			var wheels = AddComponent(vehicle, new Wheels(_container, data.VehicleData.DynamicTyreRadius));
			var breaks = AddComponent(wheels, new Breaks(_container));
			var tmp = AddComponent(breaks, new AxleGear(_container, data.GearboxData.AxleGearData));

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

			// gearbox --> clutch
			tmp = AddComponent(tmp, new Clutch(_container, data.EngineData));


			// clutch --> direct aux --> ... --> aux_XXX --> directAux
			if (data.Aux != null) {
				var aux = new Auxiliary(_container);
				foreach (var auxData in data.Aux) {
					switch (auxData.DemandType) {
						case AuxiliaryDemandType.Constant:
							aux.AddConstant(auxData.ID, auxData.PowerDemand);
							break;
						case AuxiliaryDemandType.Direct:
							aux.AddDirect(cycle);
							break;
						case AuxiliaryDemandType.Mapping:
							aux.AddMapping(auxData.ID, cycle, auxData.Data);
							break;
					}
					_dataWriter.AddAuxiliary(auxData.ID);
				}
				tmp = AddComponent(tmp, aux);
			}
			// connect aux --> engine
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

		protected virtual IDriver AddComponent(IDrivingCycle prev, IDriver next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IVehicle AddComponent(IDriver prev, IVehicle next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IWheels AddComponent(IFvInProvider prev, IWheels next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}


		protected virtual IPowerTrainComponent AddComponent(IWheels prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual IPowerTrainComponent AddComponent(IPowerTrainComponent prev, IPowerTrainComponent next)
		{
			prev.InPort().Connect(next.OutPort());
			return next;
		}

		protected virtual void AddComponent(IPowerTrainComponent prev, ITnOutProvider next)
		{
			prev.InPort().Connect(next.OutPort());
		}


		private VehicleContainer BuildEngineOnly(VectoRunData data)
		{
			var cycle = new EngineOnlyDrivingCycle(_container, data.Cycle);

			var gearbox = new EngineOnlyGearbox(_container);
			cycle.InPort().Connect(gearbox.OutPort());


			var directAux = new Auxiliary(_container);
			directAux.AddDirect(cycle);
			gearbox.InPort().Connect(directAux.OutPort());

			var engine = new EngineOnlyCombustionEngine(_container, data.EngineData);
			directAux.InPort().Connect(engine.OutPort());

			return _container;
		}
	}
}
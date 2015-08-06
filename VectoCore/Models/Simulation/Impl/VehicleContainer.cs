﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class VehicleContainer : IVehicleContainer
	{
		internal readonly IList<VectoSimulationComponent> Components = new List<VectoSimulationComponent>();
		internal IEngineInfo Engine;
		internal IGearboxInfo Gearbox;
		internal IVehicleInfo Vehicle;
		internal IBreaks Breaks;

		internal IMileageCounter MilageCounter;

		internal IRoadLookAhead Road;

		internal ISimulationOutPort Cycle;

		internal ISummaryDataWriter SumWriter;
		internal IModalDataWriter DataWriter;

		private readonly ILog _logger;

		#region IGearCockpit

		public uint Gear()
		{
			if (Gearbox == null) {
				throw new VectoException("no gearbox available!");
			}
			return Gearbox.Gear();
		}

		#endregion

		#region IEngineCockpit

		public PerSecond EngineSpeed()
		{
			if (Engine == null) {
				throw new VectoException("no engine available!");
			}
			return Engine.EngineSpeed();
		}

		#endregion

		#region IVehicleCockpit

		public MeterPerSecond VehicleSpeed()
		{
			return Vehicle != null ? Vehicle.VehicleSpeed() : 0.SI<MeterPerSecond>();
		}

		public Kilogram VehicleMass()
		{
			return Vehicle != null ? Vehicle.VehicleMass() : 0.SI<Kilogram>();
		}

		public Kilogram VehicleLoading()
		{
			return Vehicle != null ? Vehicle.VehicleLoading() : 0.SI<Kilogram>();
		}

		public Kilogram TotalMass()
		{
			return Vehicle != null ? Vehicle.TotalMass() : 0.SI<Kilogram>();
		}

		#endregion

		public VehicleContainer(IModalDataWriter dataWriter = null, ISummaryDataWriter sumWriter = null)
		{
			_logger = LogManager.GetLogger(GetType());
			DataWriter = dataWriter;
			SumWriter = sumWriter;
		}

		#region IVehicleContainer

		public ISimulationOutPort GetCycleOutPort()
		{
			return Cycle;
		}

		public virtual void AddComponent(VectoSimulationComponent component)
		{
			Components.Add(component);

			var engine = component as IEngineInfo;
			if (engine != null) {
				Engine = engine;
			}

			var gearbox = component as IGearboxInfo;
			if (gearbox != null) {
				Gearbox = gearbox;
			}

			var vehicle = component as IVehicleInfo;
			if (vehicle != null) {
				Vehicle = vehicle;
			}

			var cycle = component as ISimulationOutPort;
			if (cycle != null) {
				Cycle = cycle;
			}

			var milage = component as IMileageCounter;
			if (milage != null) {
				MilageCounter = milage;
			}

			var breaks = component as IBreaks;
			if (breaks != null) {
				Breaks = breaks;
			}

			var road = component as IRoadLookAhead;
			if (road != null) {
				Road = road;
			}
		}


		public void CommitSimulationStep(Second time, Second simulationInterval)
		{
			_logger.Info("VehicleContainer committing simulation.");
			foreach (var component in Components) {
				component.CommitSimulationStep(DataWriter);
			}

			if (DataWriter != null) {
				DataWriter[ModalResultField.time] = time + simulationInterval / 2;
				DataWriter[ModalResultField.simulationInterval] = simulationInterval;
				DataWriter.CommitSimulationStep();
			}
		}

		public void FinishSimulation()
		{
			_logger.Info("VehicleContainer finishing simulation.");
			DataWriter.Finish();

			SumWriter.Write(DataWriter, VehicleMass(), VehicleLoading());
		}

		#endregion

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(Components);
		}

		public Meter Distance()
		{
			return MilageCounter.Distance();
		}

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Meter lookaheadDistance)
		{
			return Road.LookAhead(lookaheadDistance);
		}

		public IReadOnlyList<DrivingCycleData.DrivingCycleEntry> LookAhead(Second time)
		{
			return Road.LookAhead(time);
		}

		public Watt BreakPower
		{
			get { return Breaks.BreakPower; }
			set { Breaks.BreakPower = value; }
		}
	}
}
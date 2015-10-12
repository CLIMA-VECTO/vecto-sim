using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
	public class VehicleContainer : LoggingObject, IVehicleContainer
	{
		internal readonly IList<VectoSimulationComponent> Components = new List<VectoSimulationComponent>();
		internal IEngineInfo Engine;
		internal IGearboxInfo Gearbox;
		internal IVehicleInfo Vehicle;
		internal IBrakes Brakes;
		internal IDriverInfo Driver;

		internal IMileageCounter MilageCounter;

		internal IClutchInfo Clutch;

		internal IRoadLookAhead Road;

		internal ISimulationOutPort Cycle;

		internal ISummaryDataWriter SumWriter;
		internal IModalDataWriter DataWriter;

		#region IGearCockpit

		public uint Gear
		{
			get
			{
				if (Gearbox == null) {
					throw new VectoException("no gearbox available!");
				}
				return Gearbox.Gear;
			}
		}

		[DebuggerHidden]
		public MeterPerSecond StartSpeed
		{
			get
			{
				if (Gearbox == null) {
					throw new VectoException("No Gearbox available. StartSpeed unkown");
				}
				return Gearbox.StartSpeed;
			}
		}

		[DebuggerHidden]
		public MeterPerSquareSecond StartAcceleration
		{
			get
			{
				if (Gearbox == null) {
					throw new VectoException("No Gearbox available. StartAcceleration unknown.");
				}
				return Gearbox.StartAcceleration;
			}
		}

		#endregion

		#region IEngineCockpit

		public PerSecond EngineSpeed
		{
			get
			{
				if (Engine == null) {
					throw new VectoException("no engine available!");
				}
				return Engine.EngineSpeed;
			}
		}

		public Watt EngineStationaryFullPower(PerSecond angularSpeed)
		{
			return Engine.EngineStationaryFullPower(angularSpeed);
		}

		public PerSecond EngineIdleSpeed
		{
			get { return Engine.EngineIdleSpeed; }
		}

		public PerSecond EngineRatedSpeed
		{
			get { return Engine.EngineRatedSpeed; }
		}

		#endregion

		#region IVehicleCockpit

		public MeterPerSecond VehicleSpeed
		{
			get { return Vehicle != null ? Vehicle.VehicleSpeed : 0.SI<MeterPerSecond>(); }
		}

		public Kilogram VehicleMass
		{
			get { return Vehicle != null ? Vehicle.VehicleMass : 0.SI<Kilogram>(); }
		}

		public Kilogram VehicleLoading
		{
			get { return Vehicle != null ? Vehicle.VehicleLoading : 0.SI<Kilogram>(); }
		}

		public Kilogram TotalMass
		{
			get { return Vehicle != null ? Vehicle.TotalMass : 0.SI<Kilogram>(); }
		}

		#endregion

		public VehicleContainer(IModalDataWriter dataWriter = null, ISummaryDataWriter sumWriter = null)
		{
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

			var driver = component as IDriverInfo;
			if (driver != null) {
				Driver = driver;
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

			var breaks = component as IBrakes;
			if (breaks != null) {
				Brakes = breaks;
			}

			var road = component as IRoadLookAhead;
			if (road != null) {
				Road = road;
			}

			var clutch = component as IClutchInfo;
			if (clutch != null) {
				Clutch = clutch;
			}
		}


		public void CommitSimulationStep(Second time, Second simulationInterval)
		{
			Log.Info("VehicleContainer committing simulation. time: {0}, dist: {1}, speed: {2}", time, Distance, VehicleSpeed);
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
			Log.Info("VehicleContainer finishing simulation.");
			DataWriter.Finish();

			SumWriter.Write(DataWriter, VehicleMass, VehicleLoading);
		}

		#endregion

		public IReadOnlyCollection<VectoSimulationComponent> SimulationComponents()
		{
			return new ReadOnlyCollection<VectoSimulationComponent>(Components);
		}

		public Meter Distance
		{
			get
			{
				if (MilageCounter == null) {
					Log.Warn("No MileageCounter in VehicleContainer. Distance cannot be measured.");
					return 0.SI<Meter>();
				}
				return MilageCounter.Distance;
			}
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
			get { return Brakes.BreakPower; }
			set { Brakes.BreakPower = value; }
		}

		public bool ClutchClosed(Second absTime)
		{
			if (Clutch == null) {
				Log.Warn("No Clutch in VehicleContainer. ClutchClosed set to constant true!");
				return true;
			}
			return Clutch.ClutchClosed(absTime);
		}

		public bool VehicleStopped
		{
			get { return Driver.VehicleStopped; }
		}
	}
}
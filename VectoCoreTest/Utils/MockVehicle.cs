using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Utils
{
	public class MockVehicle : VectoSimulationComponent, IVehicle, IFvInPort, IDriverDemandOutPort, IMileageCounter
	{
		internal MeterPerSecond MyVehicleSpeed;
		internal IFvOutPort NextComponent;

		internal RequestData LastRequest = new RequestData();

		public MockVehicle(IVehicleContainer cockpit) : base(cockpit) {}
		protected override void DoWriteModalResults(IModalDataWriter writer) {}

		protected override void DoCommitSimulationStep() {}

		public IFvInPort InPort()
		{
			return this;
		}

		public IDriverDemandOutPort OutPort()
		{
			return this;
		}

		public MeterPerSecond VehicleSpeed()
		{
			return MyVehicleSpeed;
		}

		public Kilogram VehicleMass()
		{
			return 7500.SI<Kilogram>();
		}

		public Kilogram VehicleLoading()
		{
			return 0.SI<Kilogram>();
		}

		public Kilogram TotalMass()
		{
			return VehicleMass();
		}

		public void Connect(IFvOutPort other)
		{
			NextComponent = other;
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSquareSecond acceleration, Radian gradient, bool b)
		{
			LastRequest = new RequestData {
				abstime = absTime,
				dt = dt,
				acceleration = acceleration,
				gradient = gradient
			};
			return new ResponseSuccess();
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			throw new NotImplementedException();
		}

		public class RequestData
		{
			public Second abstime;
			public Second dt;
			public MeterPerSquareSecond acceleration;
			public Radian gradient;
		}

		public Meter Distance
		{
			get { return 0.SI<Meter>(); }
		}
	}
}
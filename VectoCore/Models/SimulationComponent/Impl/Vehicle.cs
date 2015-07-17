using System;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Vehicle : VectoSimulationComponent, IVehicle, IFvInPort, IDriverDemandOutPort
	{
		private IFvOutPort _nextInstance;

		private VehicleState _previousState;
		private VehicleState _currentState;

		private readonly VehicleData _data;

		public Vehicle(VehicleContainer container, VehicleData data) : base(container)
		{
			_data = data;
			_previousState = new VehicleState { Velocity = 0.SI<MeterPerSecond>() };
			_currentState = new VehicleState();
		}

		public Vehicle(VehicleContainer container, VehicleData data, double initialVelocity) : this(container, data)
		{
			_previousState.Velocity = initialVelocity.SI<MeterPerSecond>();
		}

		public IFvInPort InPort()
		{
			return this;
		}

		public IDriverDemandOutPort OutPort()
		{
			return this;
		}

		public void Connect(IFvOutPort other)
		{
			_nextInstance = other;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			throw new NotImplementedException();
		}

		protected override void DoCommitSimulationStep()
		{
			_previousState = _currentState;
			_currentState = new VehicleState();
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient)
		{
			_currentState.Velocity = _previousState.Velocity +
									(accelleration * (dt.TotalSeconds / 2.0).SI<Second>()).Cast<MeterPerSecond>();

			// DriverAcceleration = vehicleAccelerationForce - RollingResistance - AirDragResistance - SlopeResistance
			var vehicleAccelerationForce = DriverAcceleration(accelleration) + RollingResistance(gradient) +
											AirDragResistance() +
											SlopeResistance(gradient);

			return _nextInstance.Request(absTime, dt, vehicleAccelerationForce, _currentState.Velocity);
		}

		protected Newton RollingResistance(Radian gradient)
		{
			return (Math.Cos(gradient.Value()) * _data.TotalVehicleWeight() *
					Physics.GravityAccelleration *
					_data.TotalRollResistanceCoefficient).Cast<Newton>();
		}


		protected Newton AirDragResistance()
		{
			// TODO different types of cross-wind correction...
			var vAir = _currentState.Velocity;
			var Cd = _data.DragCoefficient;
			switch (_data.CrossWindCorrection) {
				case CrossWindCorrectionMode.SpeedDependent:
					//Cd = 
					break;
				case CrossWindCorrectionMode.VAirBeta:
					break;
			}

			return (Cd * _data.CrossSectionArea * Physics.AirDensity / 2 * vAir * vAir).Cast<Newton>();
		}

		protected Newton DriverAcceleration(MeterPerSquareSecond accelleration)
		{
			return ((_data.TotalVehicleWeight() + _data.ReducedMassWheels) * accelleration).Cast<Newton>();
		}


		protected Newton SlopeResistance(Radian gradient)
		{
			return (_data.TotalVehicleWeight() * Physics.GravityAccelleration * Math.Sin(gradient.Value())).Cast<Newton>();
		}

		public MeterPerSecond VehicleSpeed()
		{
			return _previousState.Velocity;
		}

		public Kilogram VehicleMass()
		{
			return _data.TotalCurbWeight();
		}

		public Kilogram VehicleLoading()
		{
			return _data.Loading;
		}

		public Kilogram TotalMass()
		{
			return _data.TotalVehicleWeight();
		}


		public class VehicleState
		{
			public MeterPerSecond Velocity;
		}
	}
}
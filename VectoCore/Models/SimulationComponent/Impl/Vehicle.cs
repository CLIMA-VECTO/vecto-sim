using System;
using System.Collections.Generic;
using System.Linq;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Declaration;
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

		public IDriverDemandOutPort OutShaft()
		{
			return this;
		}

		public void Connect(IFvOutPort other)
		{
			_nextInstance = other;
		}

		public override void CommitSimulationStep(IModalDataWriter writer)
		{
			_previousState = _currentState;
			_currentState = new VehicleState();
		}

		public IResponse Request(TimeSpan absTime, TimeSpan dt, MeterPerSquareSecond accelleration, Radian gradient)
		{
			_currentState.Velocity = _previousState.Velocity +
									(accelleration * (dt.TotalSeconds / 2.0).SI<Second>()).Cast<MeterPerSecond>();
			var force = RollingResistance(gradient) + AirDragResistance() + AccelerationForce(accelleration) +
						SlopeResistance(gradient);

			return _nextInstance.Request(absTime, dt, force, _currentState.Velocity);
		}

		protected Newton RollingResistance(Radian gradient)
		{
			return (Math.Cos(gradient.Double()) * _data.TotalVehicleWeight() *
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

		protected void CalculateAirResistanceCurve()
		{
			// todo: get from vehicle or move whole procedure to vehicle
			var cdA0Actual = 0;

			var values = DeclarationData.AirDrag.Lookup(_data.VehicleCategory);

			var betas = new List<double>();
			var deltaCdAs = new List<double>();
			for (var beta = 0; beta <= 12; beta++) {
				betas.Add(beta);
				var deltaCdA = values.A1 * beta + values.A2 * beta * beta + values.A3 * beta * beta * beta;
				deltaCdAs.Add(deltaCdA);
			}

			var cdX = new List<double> { 0 };
			var cdY = new List<double> { 0 };

			for (var vVeh = 60; vVeh <= 100; vVeh += 5) {
				var cdASum = 0.0;
				for (var alpha = 0; alpha <= 180; alpha += 10) {
					var vWindX = Physics.BaseWindSpeed * Math.Cos(alpha * Math.PI / 180);
					var vWindY = Physics.BaseWindSpeed * Math.Sin(alpha * Math.PI / 180);
					var vAirX = vVeh + vWindX;
					var vAirY = vWindY;
					var vAir = VectoMath.Sqrt<MeterPerSecond>(vAirX * vAirX + vAirY * vAirY);
					var beta = Math.Atan((vAirY / vAirX).Double()) * 180 / Math.PI;

					var k = 1;
					if (betas.First() < beta) {
						k = 0;
						while (betas[k] < beta && k < betas.Count) {
							k++;
						}
					}

					var deltaCdA = VectoMath.Interpolate(betas[k - 1], betas[k], deltaCdAs[k - 1], deltaCdAs[k], beta);

					var cdA = cdA0Actual + deltaCdA;

					var share = 10 / 180;
					if (vVeh == 0 || vVeh == 180) {
						share /= 2;
					}
					cdASum += share * cdA * (vAir * vAir / (vVeh * vVeh)).Double();
				}
				cdX.Add(vVeh);
				cdY.Add(cdASum);
			}

			cdY[0] = cdY[1];
		}


		protected Newton AccelerationForce(MeterPerSquareSecond accelleration)
		{
			return ((_data.TotalVehicleWeight() + _data.ReducedMassWheels) * accelleration).Cast<Newton>();
		}


		protected Newton SlopeResistance(Radian gradient)
		{
			return (_data.TotalVehicleWeight() * Physics.GravityAccelleration * Math.Sin(gradient.Double())).Cast<Newton>();
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
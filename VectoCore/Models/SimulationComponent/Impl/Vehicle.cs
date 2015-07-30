using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Vehicle : VectoSimulationComponent, IVehicle, IMileageCounter, IFvInPort, IDriverDemandOutPort
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
			writer[ModalResultField.v_act] = (_previousState.Velocity + _currentState.Velocity) / 2;
			writer[ModalResultField.dist] = (_previousState.Distance - _currentState.Distance) / 2;

			// hint: take care to use correct velocity when writing the P... values in moddata
		}

		protected override void DoCommitSimulationStep()
		{
			_previousState = _currentState;
			_currentState = new VehicleState();
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSquareSecond accelleration, Radian gradient,
			bool dryRun = false)
		{
			_currentState.Velocity = (_previousState.Velocity + (accelleration * dt)).Cast<MeterPerSecond>();
			_currentState.dt = dt;
			_currentState.Distance = ((_previousState.Velocity + _currentState.Velocity) / 2 * _currentState.dt).Cast<Meter>();

			// DriverAcceleration = vehicleAccelerationForce - RollingResistance - AirDragResistance - SlopeResistance
			var vehicleAccelerationForce = DriverAcceleration(accelleration) + RollingResistance(gradient) +
											AirDragResistance() +
											SlopeResistance(gradient);

			return _nextInstance.Request(absTime, dt, vehicleAccelerationForce, _currentState.Velocity);
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			_previousState = new VehicleState() { Distance = 0.SI<Meter>(), Velocity = 0.SI<MeterPerSecond>() };
			_currentState = new VehicleState() { Distance = 0.SI<Meter>(), Velocity = 0.SI<MeterPerSecond>() };

			var vehicleAccelerationForce = RollingResistance(roadGradient) + AirDragResistance() + SlopeResistance(roadGradient);
			return _nextInstance.Initialize(vehicleAccelerationForce, vehicleSpeed);
		}

		protected Newton RollingResistance(Radian gradient)
		{
			return (Math.Cos(gradient.Value()) * _data.TotalVehicleWeight() *
					Physics.GravityAccelleration *
					_data.TotalRollResistanceCoefficient).Cast<Newton>();
		}


		protected Newton AirDragResistance()
		{
			var vAverage = (_previousState.Velocity + _currentState.Velocity) / 2;

			var vAir = vAverage;

			// todo: different CdA in different file versions!
			var CdA = _data.CrossSectionArea * _data.DragCoefficient;

			switch (_data.CrossWindCorrectionMode) {
				case CrossWindCorrectionMode.NoCorrection:
					break;

				case CrossWindCorrectionMode.SpeedDependent:
					var values = DeclarationData.AirDrag.Lookup(_data.VehicleCategory);
					var curve = CalculateAirResistanceCurve(values);
					CdA *= AirDragInterpolate(curve, vAverage);
					break;

				// todo ask raphael: What is the air cd decl mode?
				//case tCdMode.CdOfVdecl
				//	  CdA = AirDragInterpolate(curve, vAverage);
				//	  break;

				case CrossWindCorrectionMode.VAirBeta:
					//todo: get data from driving cycle
					//vAir = DrivingCycleData.DrivingCycleEntry.AirSpeedRelativeToVehicle;
					//CdA *= AirDragInterpolate(Math.Abs(DrivingCycleData.DrivingCycleEntry.WindYawAngle))
					throw new NotImplementedException("VAirBeta is not implemented");
				//break;
			}

			return (CdA * Physics.AirDensity / 2 * vAir * vAir).Cast<Newton>();
		}

		private double AirDragInterpolate(IEnumerable<Point> curve, MeterPerSecond x)
		{
			var p = curve.GetSamples(c => c.X < x);

			if (x < p.Item1.X || p.Item2.X < x) {
				Log.Error(_data.CrossWindCorrectionMode == CrossWindCorrectionMode.VAirBeta
					? string.Format("CdExtrapol β = {0}", x)
					: string.Format("CdExtrapol v = {0}", x));
			}

			return VectoMath.Interpolate(p.Item1.X, p.Item2.X, p.Item1.Y, p.Item2.Y, x);
		}

		public class Point
		{
			public MeterPerSecond X;
			public double Y;
		}

		protected Point[] CalculateAirResistanceCurve(AirDrag.AirDragEntry values)
		{
			// todo: get from vehicle or move whole procedure to vehicle
			var cdA0Actual = 0;

			var betaValues = new Dictionary<int, double>();
			for (var beta = 0; beta <= 12; beta++) {
				var deltaCdA = values.A1 * beta + values.A2 * beta * beta + values.A3 * beta * beta * beta;
				betaValues[beta] = deltaCdA;
			}

			var points = new List<Point> { new Point { X = 0.SI<MeterPerSecond>(), Y = 0 } };

			for (var vVeh = 60; vVeh <= 100; vVeh += 5) {
				var cdASum = 0.0;
				for (var alpha = 0; alpha <= 180; alpha += 10) {
					var vWindX = Physics.BaseWindSpeed * Math.Cos(alpha.ToRadian());
					var vWindY = Physics.BaseWindSpeed * Math.Sin(alpha.ToRadian());
					var vAirX = vVeh + vWindX;
					var vAirY = vWindY;
					var vAir = VectoMath.Sqrt<MeterPerSecond>(vAirX * vAirX + vAirY * vAirY);
					var beta = Math.Atan((vAirY / vAirX).Value()).ToDegree();

					var sec = betaValues.GetSamples(b => b.Key < beta);
					var deltaCdA = VectoMath.Interpolate(sec.Item1.Key, sec.Item2.Key, sec.Item1.Value, sec.Item2.Value, beta);
					var cdA = cdA0Actual + deltaCdA;

					var degreeShare = ((vVeh != 0 && vVeh != 180) ? 10.0 / 180.0 : 5.0 / 180.0);

					cdASum += degreeShare * cdA * (vAir * vAir / (vVeh * vVeh)).Scalar();
				}
				points.Add(new Point { X = vVeh.SI<MeterPerSecond>(), Y = cdASum });
			}

			points[0].Y = points[1].Y;
			return points.ToArray();
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
			public Second dt;
			public Meter Distance;
		}

		public Meter Distance()
		{
			return _previousState.Distance;
		}
	}
}
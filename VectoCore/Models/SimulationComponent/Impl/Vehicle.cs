using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.Simulation;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public class Vehicle : VectoSimulationComponent, IVehicle, IMileageCounter, IFvInPort, IDriverDemandOutPort
	{
		protected IFvOutPort NextComponent;
		private VehicleState _previousState;
		private VehicleState _currentState;
		private readonly VehicleData _data;

		private readonly Point[] _airResistanceCurve;

		public MeterPerSecond VehicleSpeed
		{
			get { return _previousState.Velocity; }
		}

		public Kilogram VehicleMass
		{
			get { return _data.TotalCurbWeight(); }
		}

		public Kilogram VehicleLoading
		{
			get { return _data.Loading; }
		}

		public Kilogram TotalMass
		{
			get { return _data.TotalVehicleWeight(); }
		}

		public Meter Distance
		{
			get { return _previousState.Distance; }
		}


		public Vehicle(IVehicleContainer container, VehicleData data) : base(container)
		{
			_data = data;
			_previousState = new VehicleState { Distance = 0.SI<Meter>(), Velocity = 0.SI<MeterPerSecond>() };
			_currentState = new VehicleState { Distance = 0.SI<Meter>(), Velocity = 0.SI<MeterPerSecond>() };
		}

		public Vehicle(IVehicleContainer container, VehicleData data, double initialVelocity) : this(container, data)
		{
			var values = DeclarationData.AirDrag.Lookup(_data.VehicleCategory);
			_airResistanceCurve = CalculateAirResistanceCurve(values);

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
			NextComponent = other;
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, Radian roadGradient)
		{
			_previousState = new VehicleState {
				Distance = 0.SI<Meter>(),
				Velocity = vehicleSpeed,
				AirDragResistance = AirDragResistance(0.SI<MeterPerSquareSecond>(), Constants.SimulationSettings.TargetTimeInterval),
				RollingResistance = RollingResistance(roadGradient),
				SlopeResistance = SlopeResistance(roadGradient)
			};
			_previousState.VehicleAccelerationForce = _previousState.RollingResistance
													+ _previousState.AirDragResistance
													+ _previousState.SlopeResistance;

			_currentState = new VehicleState {
				Distance = 0.SI<Meter>(),
				Velocity = vehicleSpeed,
				AirDragResistance = _previousState.AirDragResistance,
				RollingResistance = _previousState.RollingResistance,
				SlopeResistance = _previousState.SlopeResistance,
				VehicleAccelerationForce = _previousState.VehicleAccelerationForce,
			};


			return NextComponent.Initialize(_currentState.VehicleAccelerationForce, vehicleSpeed);
		}

		public IResponse Initialize(MeterPerSecond vehicleSpeed, MeterPerSquareSecond startAcceleration, Radian roadGradient)
		{
			var tmp = _previousState.Velocity;
			// set vehicle speed to get accurate airdrag resistance
			_previousState.Velocity = vehicleSpeed;
			_currentState.Velocity = vehicleSpeed + startAcceleration * Constants.SimulationSettings.TargetTimeInterval;
			var vehicleAccelerationForce = DriverAcceleration(startAcceleration) + RollingResistance(roadGradient) +
											AirDragResistance(startAcceleration, Constants.SimulationSettings.TargetTimeInterval) +
											SlopeResistance(roadGradient);

			var retVal = NextComponent.Initialize(vehicleAccelerationForce, vehicleSpeed);

			_previousState.Velocity = tmp;
			_currentState.Velocity = tmp;
			return retVal;
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSquareSecond acceleration, Radian gradient,
			bool dryRun = false)
		{
			Log.Debug("from Wheels: acceleration: {0}", acceleration);
			_currentState.dt = dt;
			_currentState.Acceleration = acceleration;
			_currentState.Velocity = _previousState.Velocity + acceleration * dt;
			_currentState.Distance = _previousState.Distance + dt * (_previousState.Velocity + _currentState.Velocity) / 2;

			_currentState.DriverAcceleration = DriverAcceleration(acceleration);
			_currentState.RollingResistance = RollingResistance(gradient);
			_currentState.AirDragResistance = AirDragResistance(acceleration, dt);
			_currentState.SlopeResistance = SlopeResistance(gradient);

			// DriverAcceleration = vehicleAccelerationForce - RollingResistance - AirDragResistance - SlopeResistance
			_currentState.VehicleAccelerationForce = _currentState.DriverAcceleration
													+ _currentState.RollingResistance
													+ _currentState.AirDragResistance
													+ _currentState.SlopeResistance;

			var retval = NextComponent.Request(absTime, dt, _currentState.VehicleAccelerationForce, _currentState.Velocity,
				dryRun);
			return retval;
		}

		protected override void DoWriteModalResults(IModalDataWriter writer)
		{
			var averageVelocity = (_previousState.Velocity + _currentState.Velocity) / 2;

			writer[ModalResultField.v_act] = averageVelocity;
			writer[ModalResultField.PaVeh] = (_previousState.VehicleAccelerationForce * _previousState.Velocity +
											_currentState.VehicleAccelerationForce * _currentState.Velocity) / 2;
			writer[ModalResultField.Pgrad] = (_previousState.SlopeResistance * _previousState.Velocity +
											_currentState.SlopeResistance * _currentState.Velocity) / 2;
			writer[ModalResultField.Proll] = (_previousState.RollingResistance * _previousState.Velocity +
											_currentState.RollingResistance * _currentState.Velocity) / 2;
			// TODO: comptuation of AirResistancePower is wrong!

			Watt averageAirDragPower;
			if (_currentState.Acceleration.IsEqual(0)) {
				var vAverage = (_previousState.Velocity + _currentState.Velocity) / 2;
				averageAirDragPower = (ComputeAirDragForce(vAverage) * vAverage).Cast<Watt>();
			} else {
				// compute the average force within the current simulation interval
				// P(t) = k * v(t)^3  , v(t) = v0 + a * t  // a != 0
				// => P_avg = 1/T * k/(4*a) * (v1^4 - v0^4) = 1/(4*a*T)(F1*v1^2 - F0*v0^2)
				var force0 = ComputeAirDragForce(_previousState.Velocity);
				var force1 = ComputeAirDragForce(_currentState.Velocity);
				averageAirDragPower = ((force1 * _currentState.Velocity * _currentState.Velocity -
										force0 * _previousState.Velocity * _previousState.Velocity) /
										(4 * _currentState.Acceleration * _currentState.dt)).Cast<Watt>();
			}
			writer[ModalResultField.Pair] = averageAirDragPower;


			// sanity check: is the vehicle in step with the cycle?
			if (writer[ModalResultField.dist] == DBNull.Value) {
				Log.Warn("distance field is not set!");
			} else {
				var distance = (SI)writer[ModalResultField.dist];
				if (!distance.IsEqual(_currentState.Distance, 1e-12.SI<Meter>())) {
					Log.Warn("distance diverges: {0}, distance: {1}", (distance - _currentState.Distance).Value(), distance);
				}
			}
		}

		protected override void DoCommitSimulationStep()
		{
			_previousState = _currentState;
			_currentState = new VehicleState();
		}

		protected Newton RollingResistance(Radian gradient)
		{
			var retVal = (Math.Cos(gradient.Value()) * _data.TotalVehicleWeight() *
						Physics.GravityAccelleration *
						_data.TotalRollResistanceCoefficient).Cast<Newton>();
			Log.Debug("RollingResistance: {0}", retVal);
			return retVal;
		}


		protected Newton AirDragResistance(MeterPerSquareSecond acceleration, Second dt)
		{
			if (acceleration.IsEqual(0)) {
				var vAverage = (_previousState.Velocity + _currentState.Velocity) / 2;
				var retVal = ComputeAirDragForce(vAverage);
				Log.Debug("AirDragResistance: {0}", retVal);
				return retVal;
			}

			// compute the average force within the current simulation interval
			// F(t) = k * v(t)^2  , v(t) = v0 + a * t  // a != 0
			// => F_avg = 1/T * k/(3*a) * (v1^3 - v0^3) = 1/(3*a*T)(F1*v1 - F0*v0)
			var force0 = ComputeAirDragForce(_previousState.Velocity);
			var force1 = ComputeAirDragForce(_currentState.Velocity);

			var result =
				((force1 * _currentState.Velocity - force0 * _previousState.Velocity) / (3.0 * acceleration * dt)).Cast<Newton>();

			Log.Debug("AirDragResistance: {0}", result);
			return result;
		}

		protected Newton ComputeAirDragForce(MeterPerSecond velocity)
		{
			var CdA = ComputeEffectiveAirDragArea(velocity);
			return (Physics.AirDensity / 2.0 * CdA * velocity * velocity).Cast<Newton>();
		}

		private SquareMeter ComputeEffectiveAirDragArea(MeterPerSecond velocity)
		{
			var CdA = _data.AerodynamicDragAera;
			switch (_data.CrossWindCorrectionMode) {
				case CrossWindCorrectionMode.NoCorrection:
					break;
				case CrossWindCorrectionMode.DeclarationModeCorrection:
					CdA = AirDragInterpolate(velocity);
					break;
				default:
					throw new NotImplementedException(string.Format(" is not implemented", _data.CrossWindCorrectionMode));
			}
			return CdA;
		}

		private SquareMeter AirDragInterpolate(MeterPerSecond x)
		{
			var p = _airResistanceCurve.GetSection(c => c.X < x);

			if (x < p.Item1.X || p.Item2.X < x) {
				Log.Error(_data.CrossWindCorrectionMode == CrossWindCorrectionMode.VAirBetaLookupTable
					? string.Format("CdExtrapol β = {0}", x)
					: string.Format("CdExtrapol v = {0}", x));
			}

			return VectoMath.Interpolate(p.Item1.X, p.Item2.X, p.Item1.Y, p.Item2.Y, x);
		}

		public class Point
		{
			public MeterPerSecond X;
			public SquareMeter Y;
		}

		protected Point[] CalculateAirResistanceCurve(AirDrag.AirDragEntry values)
		{
			// todo: get from vehicle or move whole procedure to vehicle
			var cdA0Actual = _data.AerodynamicDragAera;

			var betaValues = new Dictionary<int, SquareMeter>();
			for (var beta = 0; beta <= 12; beta++) {
				var deltaCdA = values.A1 * beta + values.A2 * beta * beta + values.A3 * beta * beta * beta;
				betaValues[beta] = deltaCdA.SI<SquareMeter>();
			}

			var points = new List<Point> { new Point { X = 0.SI<MeterPerSecond>(), Y = 0.SI<SquareMeter>() } };

			for (var vVeh = 60.KMPHtoMeterPerSecond(); vVeh <= 100.KMPHtoMeterPerSecond(); vVeh += 5.KMPHtoMeterPerSecond()) {
				var cdASum = 0.0.SI<SquareMeter>();
				for (var alpha = 0; alpha <= 180; alpha += 10) {
					var vWindX = Physics.BaseWindSpeed * Math.Cos(alpha.ToRadian());
					var vWindY = Physics.BaseWindSpeed * Math.Sin(alpha.ToRadian());
					var vAirX = vVeh + vWindX;
					var vAirY = vWindY;
					var vAir = VectoMath.Sqrt<MeterPerSecond>(vAirX * vAirX + vAirY * vAirY);
					var beta = Math.Atan((vAirY / vAirX).Value()).ToDegree();

					var sec = betaValues.GetSection(b => b.Key < beta);
					var deltaCdA = VectoMath.Interpolate(sec.Item1.Key, sec.Item2.Key, sec.Item1.Value, sec.Item2.Value, beta);
					var cdA = cdA0Actual + deltaCdA;

					var degreeShare = ((alpha != 0 && alpha != 180) ? 10.0 / 180.0 : 5.0 / 180.0);

					cdASum += degreeShare * cdA * (vAir * vAir / (vVeh * vVeh)).Cast<Scalar>();
				}
				points.Add(new Point { X = vVeh, Y = cdASum });
			}

			points[0].Y = points[1].Y;
			return points.ToArray();
		}

		protected Newton DriverAcceleration(MeterPerSquareSecond accelleration)
		{
			var retVal = ((_data.TotalVehicleWeight() + _data.ReducedMassWheels) * accelleration).Cast<Newton>();
			Log.Debug("DriverAcceleration: {0}", retVal);
			return retVal;
		}


		protected Newton SlopeResistance(Radian gradient)
		{
			var retVal = (_data.TotalVehicleWeight() * Physics.GravityAccelleration * Math.Sin(gradient.Value())).Cast<Newton>();
			Log.Debug("SlopeResistance: {0}", retVal);
			return retVal;
		}


		public class VehicleState
		{
			public MeterPerSecond Velocity;
			public Second dt;
			public Meter Distance;

			public Newton VehicleAccelerationForce;
			public Newton DriverAcceleration;
			public Newton SlopeResistance;
			public Newton AirDragResistance;
			public Newton RollingResistance;
			public MeterPerSquareSecond Acceleration { get; set; }
		}
	}
}
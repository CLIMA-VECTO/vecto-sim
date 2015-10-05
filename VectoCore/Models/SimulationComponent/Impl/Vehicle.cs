using System;
using System.Collections.Generic;
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
				AirDragResistance = AirDragResistance(),
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
			var vehicleAccelerationForce = DriverAcceleration(startAcceleration) + RollingResistance(roadGradient) +
											AirDragResistance() + SlopeResistance(roadGradient);
			return NextComponent.Initialize(vehicleAccelerationForce, vehicleSpeed);
		}

		public IResponse Request(Second absTime, Second dt, MeterPerSquareSecond accelleration, Radian gradient,
			bool dryRun = false)
		{
			Log.Debug("from Wheels: acceleration: {0}", accelleration);
			_currentState.dt = dt;
			_currentState.Velocity = _previousState.Velocity + accelleration * dt;
			_currentState.Distance = _previousState.Distance + dt * (_previousState.Velocity + _currentState.Velocity) / 2;

			_currentState.DriverAcceleration = DriverAcceleration(accelleration);
			_currentState.RollingResistance = RollingResistance(gradient);
			_currentState.AirDragResistance = AirDragResistance();
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
			writer[ModalResultField.Pair] = (_previousState.AirDragResistance * _previousState.Velocity +
											_currentState.AirDragResistance * _currentState.Velocity) / 2;


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

			var retVal = (CdA * Physics.AirDensity / 2 * vAir * vAir).Cast<Newton>();
			Log.Debug("AirDragResistance: {0}", retVal);
			return retVal;
		}

		private double AirDragInterpolate(IEnumerable<Point> curve, MeterPerSecond x)
		{
			var p = curve.GetSection(c => c.X < x);

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

			for (var vVeh = 60.KMPHtoMeterPerSecond(); vVeh <= 100; vVeh += 5.SI<MeterPerSecond>()) {
				var cdASum = 0.0;
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
		}
	}
}
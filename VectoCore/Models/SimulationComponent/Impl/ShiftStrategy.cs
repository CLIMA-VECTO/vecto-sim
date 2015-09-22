using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public abstract class ShiftStrategy : IShiftStrategy
	{
		protected IDataBus DataBus;
		protected GearboxData Data;

		public abstract uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		public abstract uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);


		public Gearbox Gearbox { get; set; }


		protected ShiftStrategy(GearboxData data, IDataBus dataBus)
		{
			DataBus = dataBus;
			Data = data;
		}

		/// <summary>
		/// Tests if the operating point is below the down-shift curve (=outside of shift curve).
		/// </summary>
		protected virtual bool IsBelowDownShiftCurve(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
		{
			if (gear <= 1) {
				return false;
			}

			var downSection = Data.Gears[gear].ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2);
		}

		/// <summary>
		/// Tests if the gearbox is above the up-shift curve (=outside of shift curve).
		/// </summary>
		protected virtual bool IsAboveUpShiftCurve(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
		{
			if (gear >= Data.Gears.Count) {
				return false;
			}

			var upSection = Data.Gears[gear].ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			return IsOnRightSide(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		/// <remarks>
		/// Computes a simplified cross product for the vectors: from-->X, from-->to and checks 
		/// if the z-component is positive (which means that X was on the right side of from-->to).
		/// </remarks>
		private static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			var z = ab.X * ac.Y - ab.Y * ac.X;
			return z.IsGreater(0);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		/// <remarks>
		/// Computes a simplified cross product for the vectors: from-->X, from-->to and checks 
		/// if the z-component is negative (which means that X was on the left side of from-->to).
		/// </remarks>
		private static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			var z = ab.X * ac.Y - ab.Y * ac.X;
			return z.IsSmaller(0);
		}
	}

	public class AMTShiftStrategy : ShiftStrategy
	{
		/// <summary>
		/// The previous gear before the disengagement. Used for GetGear() when skipGears is false.
		/// </summary>
		protected uint PreviousGear;

		public AMTShiftStrategy(GearboxData data, IDataBus dataBus) : base(data, dataBus)
		{
			PreviousGear = 1;
		}

		private uint GetGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool skipGears,
			double torqueReserve)
		{
			var maxGear = (uint)(skipGears ? Data.Gears.Count : Math.Min(PreviousGear + 1, Data.Gears.Count));
			var minGear = skipGears ? 1 : Math.Max(PreviousGear - 1, 1);

			if (outEngineSpeed.IsEqual(0)) {
				return minGear;
			}

			for (var gear = maxGear; gear > minGear; gear--) {
				Gearbox.Gear = gear;
				var response = (ResponseDryRun)Gearbox.Request(absTime, dt, outTorque, outEngineSpeed, true);
				var currentPower = response.EnginePowerRequest;

				var fullLoadPower = currentPower - response.DeltaFullLoad;
				var reserve = 1 - (currentPower / fullLoadPower).Cast<Scalar>();

				var inAngularSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
				var inTorque = currentPower / inAngularSpeed;
				if (!IsBelowDownShiftCurve(gear, inTorque, inAngularSpeed) && reserve >= torqueReserve) {
					return gear;
				}
			}
			return minGear;
		}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			return GetGear(absTime, dt, outTorque, outEngineSpeed, Data.SkipGears, Data.TorqueReserve);
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			PreviousGear = Gearbox.Gear;
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			// todo: early upshift
			return IsBelowDownShiftCurve(gear, torque, angularSpeed) || IsAboveUpShiftCurve(gear, torque, angularSpeed);
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			if (DataBus.VehicleSpeed.IsEqual(0)) {
				for (var gear = (uint)Data.Gears.Count; gear > 1; gear--) {
					var inAngularSpeed = outEngineSpeed * Data.Gears[gear].Ratio;

					if (inAngularSpeed > Data.Gears[gear].FullLoadCurve.RatedSpeed) {
						continue;
					}

					var response = Gearbox.Initialize(gear, outTorque, outEngineSpeed);

					var currentPower = response.EnginePowerRequest;
					var inTorque = currentPower / inAngularSpeed;

					var fullLoadPower = currentPower - response.DeltaFullLoad;
					var reserve = 1 - (currentPower / fullLoadPower).Cast<Scalar>();

					// if in shift curve and above idle speed and torque reserve is provided.
					if (!IsBelowDownShiftCurve(gear, inTorque, inAngularSpeed) && inAngularSpeed > DataBus.EngineIdleSpeed &&
						reserve >= Data.StartTorqueReserve) {
						return gear;
					}
				}
				return 1;
			} else {
				for (var gear = (uint)Data.Gears.Count; gear > 1; gear--) {
					var response = Gearbox.Initialize(gear, outTorque, outEngineSpeed);

					var currentPower = response.EnginePowerRequest;
					var inAngularSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
					var inTorque = currentPower / inAngularSpeed;

					var fullLoadPower = currentPower - response.DeltaFullLoad;
					var reserve = 1 - (currentPower / fullLoadPower).Cast<Scalar>();

					// if in shift curve and torque reserve is provided: return the current gear
					if (!IsBelowDownShiftCurve(gear, inTorque, inAngularSpeed) && !IsAboveUpShiftCurve(gear, inTorque, inAngularSpeed) &&
						reserve >= Data.StartTorqueReserve) {
						return gear;
					}

					// if over the up shift curve: return the previous gear (even thou it did not provide the required torque reserve)
					if (IsAboveUpShiftCurve(gear, inTorque, inAngularSpeed) && gear < Data.Gears.Count) {
						return gear + 1;
					}
				}
			}

			// fallback: return first gear
			return 1;
		}
	}

	//TODO Implement MTShiftStrategy
	public class MTShiftStrategy : ShiftStrategy
	{
		public MTShiftStrategy(GearboxData data, IDataBus bus) : base(data, bus) {}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}
	}

	// TODO Implement ATShiftStrategy
	public class ATShiftStrategy : ShiftStrategy
	{
		public ATShiftStrategy(GearboxData data, IDataBus bus) : base(data, bus) {}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}
	}

	// TODO Implement CustomShiftStrategy
	public class CustomShiftStrategy : ShiftStrategy
	{
		public CustomShiftStrategy(GearboxData data, IDataBus dataBus) : base(data, dataBus) {}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new NotImplementedException();
		}
	}
}
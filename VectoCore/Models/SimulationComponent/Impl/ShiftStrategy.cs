using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	/// <summary>
	/// Class ShiftStrategy is a base class for shift strategies. Implements some helper methods for checking the shift curves.
	/// </summary>
	public abstract class ShiftStrategy : IShiftStrategy
	{
		protected IDataBus DataBus;

		protected GearboxData Data;

		protected ShiftStrategy(GearboxData data, IDataBus dataBus)
		{
			DataBus = dataBus;
			Data = data;
		}

		public abstract uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		public abstract void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		public abstract bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque, PerSecond inAngularSpeed, uint gear, Second lastShiftTime);

		public abstract uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		public Gearbox Gearbox { get; set; }

		/// <summary>
		/// Tests if the operating point is below the down-shift curve (=outside of shift curve).
		/// </summary>
		/// <param name="gear">The gear.</param>
		/// <param name="inTorque">The in torque.</param>
		/// <param name="inEngineSpeed">The in engine speed.</param>
		/// <returns><c>true</c> if the operating point is below the down-shift curv; otherwise, <c>false</c>.</returns>
		protected virtual bool IsBelowDownShiftCurve(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
		{
			if (gear <= 1) {
				return false;
			}

			var downSection = Data.Gears[gear].ShiftPolygon.Downshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);
			if (downSection.Item2.AngularSpeed < inEngineSpeed) {
				return false;
			}

			return IsOnLeftSide(inEngineSpeed, inTorque, downSection.Item1, downSection.Item2);
		}

		/// <summary>
		/// Tests if the operating point is above the up-shift curve (=outside of shift curve).
		/// </summary>
		/// <param name="gear">The gear.</param>
		/// <param name="inTorque">The in torque.</param>
		/// <param name="inEngineSpeed">The in engine speed.</param>
		/// <returns><c>true</c> if the operating point is above the up-shift curve; otherwise, <c>false</c>.</returns>
		protected virtual bool IsAboveUpShiftCurve(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
		{
			if (gear >= Data.Gears.Count) {
				return false;
			}

			var upSection = Data.Gears[gear].ShiftPolygon.Upshift.GetSection(entry => entry.AngularSpeed < inEngineSpeed);

			if (upSection.Item2.AngularSpeed < inEngineSpeed) {
				return true;
			}

			return IsOnRightSide(inEngineSpeed, inTorque, upSection.Item1, upSection.Item2);
		}

		/// <summary>
		/// Tests if current power request is on the left side of the shiftpolygon segment
		/// </summary>
		/// <param name="angularSpeed">The angular speed.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns><c>true</c> if current power request is on the left side of the shiftpolygon segment; otherwise, <c>false</c>.</returns>
		/// <remarks>Computes a simplified cross product for the vectors: from--X, from--to and checks
		/// if the z-component is positive (which means that X was on the right side of from--to).</remarks>
		private static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			var z = ab.X * ac.Y - ab.Y * ac.X;
			return z.IsGreater(0);
		}

		/// <summary>
		/// Tests if current power request is on the left side of the shiftpolygon segment
		/// </summary>
		/// <param name="angularSpeed">The angular speed.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <returns><c>true</c> if current power request is on the left side of the shiftpolygon segment; otherwise, <c>false</c>.</returns>
		/// <remarks>Computes a simplified cross product for the vectors: from--X, from--to and checks
		/// if the z-component is negative (which means that X was on the left side of from--to).</remarks>
		private static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			var z = ab.X * ac.Y - ab.Y * ac.X;
			return z.IsSmaller(0);
		}
	}

	/// <summary>
	/// AMTShiftStrategy implements the AMT Shifting Behaviour.
	/// </summary>
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

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularSpeed)
		{
			//TODO Find gear with current outTorque and angularSpeed, but not during dry run
			return NextGear;

			//var skipGears = Data.SkipGears;
			//var requiredTorqueReserve = Data.TorqueReserve;
			//var maxGear = (uint)(skipGears ? Data.Gears.Count : Math.Min(PreviousGear + 1, Data.Gears.Count));
			//var minGear = skipGears ? 1 : Math.Max(PreviousGear - 1, 1);

			//Func<uint, bool> speedTooLowForEngine =
			//	gear => (outAngularSpeed * Data.Gears[gear].Ratio).IsSmaller(DataBus.EngineIdleSpeed);

			//// original vecto2.2: (inAngularSpeed - IdleSpeed) / (RatedSpeed - IdleSpeed) >= 1.2
			////                  =  inAngularSpeed - IdleSpeed >= 1.2*(RatedSpeed - IdleSpeed)
			////                  =  inAngularSpeed >= 1.2*RatedSpeed - 0.2*IdleSpeed
			//Func<uint, bool> speedToHighForEngine = gear =>
			//	(outAngularSpeed * Data.Gears[gear].Ratio).IsGreaterOrEqual(1.2 * DataBus.EngineRatedSpeed -
			//																0.2 * DataBus.EngineIdleSpeed);

			//while (speedTooLowForEngine(maxGear) && maxGear > minGear) {
			//	maxGear--;
			//}

			//while (speedToHighForEngine(minGear) && minGear < maxGear) {
			//	minGear++;
			//}

			//var tmpGear = Gearbox.Gear;
			//// loop only runs from maxGear to minGear+1 because minGear is returned afterwards anyway.
			//for (var gear = maxGear; gear >= minGear; gear--) {
			//	Gearbox.Gear = gear;
			//	var response = (ResponseDryRun)Gearbox.Request(absTime, dt, outTorque, outAngularSpeed, true);
			//	Gearbox.Gear = tmpGear;

			//	var currentPower = response.EnginePowerRequest;

			//	var fullLoadPower = currentPower - response.DeltaFullLoad;
			//	var torqueReserve = 1 - (currentPower / fullLoadPower).Cast<Scalar>();

			//	var inAngularSpeed = outAngularSpeed * Data.Gears[gear].Ratio;
			//	var inTorque = response.ClutchPowerRequest / inAngularSpeed;

			//	var isBelowShiftCurve = IsBelowDownShiftCurve(gear, inTorque, inAngularSpeed);
			//	var isAboveShiftCurve = IsAboveUpShiftCurve(gear, inTorque, inAngularSpeed);

			//	// if gear is the gear which was found at ShiftRequired --> take it!
			//	if (gear == NextGear && torqueReserve.IsGreaterOrEqual(0)) {
			//		return gear;
			//	}

			//	if (torqueReserve.IsGreaterOrEqual(requiredTorqueReserve) && !isBelowShiftCurve && !isAboveShiftCurve) {
			//		return gear;
			//	}

			//	// if already over the up shift curve: return the previous gear (although it did not provide the required torque reserve)
			//	if (isAboveShiftCurve && gear < maxGear) {
			//		return gear + 1;
			//	}
			//}
			//return minGear;
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			PreviousGear = Gearbox.Gear;
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			if (DataBus.VehicleSpeed.IsEqual(0)) {
				for (var gear = (uint)Data.Gears.Count; gear > 1; gear--) {
					var inAngularSpeed = outEngineSpeed * Data.Gears[gear].Ratio;

					if (inAngularSpeed > Data.Gears[gear].FullLoadCurve.RatedSpeed || inAngularSpeed.IsEqual(0)) {
						continue;
					}

					var response = Gearbox.Initialize(gear, outTorque, outEngineSpeed);

					var fullLoadPower = response.EnginePowerRequest - response.DeltaFullLoad;
					var reserve = 1 - (response.EnginePowerRequest / fullLoadPower).Cast<Scalar>();
					var inTorque = response.ClutchPowerRequest / inAngularSpeed;

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

					var inAngularSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
					var fullLoadPower = response.EnginePowerRequest - response.DeltaFullLoad;
					var reserve = 1 - (response.EnginePowerRequest / fullLoadPower).Cast<Scalar>();
					var inTorque = response.ClutchPowerRequest / inAngularSpeed;

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

		public override bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque, PerSecond inAngularVelocity, uint gear, Second lastShiftTime)
		{
			if (DataBus.VehicleStopped) {
				return false;
			}

			var speedTooLowForEngine = inAngularVelocity.IsSmaller(DataBus.EngineIdleSpeed);
			var speedToHighForEngine =
				inAngularVelocity.IsGreaterOrEqual(1.2 * DataBus.EngineRatedSpeed - 0.2 * DataBus.EngineIdleSpeed);

			if (gear > 1 && speedTooLowForEngine) {
				NextGear = gear - 1;
				return true;
			}

			if (gear < Data.Gears.Count && speedToHighForEngine) {
				NextGear = gear + 1;
				return true;
			}

			var minimumShiftTimePassed = (lastShiftTime + Data.ShiftTime).IsSmaller(absTime);
			if (!minimumShiftTimePassed) {
				return false;
			}

			var shiftDown = IsBelowDownShiftCurve(gear, inTorque, inAngularVelocity);
			var shiftUp = IsAboveUpShiftCurve(gear, inTorque, inAngularVelocity);

			if (shiftDown) {
				NextGear = gear - 1;
				return true;
			}
			if (shiftUp) {
				NextGear = gear + 1;
				return true;
			}

			if (Data.EarlyShiftUp && gear < Data.Gears.Count) {
				// try if next gear would provide enough torque reserve
				var nextGear = gear + 1;

				Gearbox.Gear = nextGear;
				var response = (ResponseDryRun)Gearbox.Request(absTime, dt, outTorque, outAngularVelocity, true);

				var nextAngularVelocity = Data.Gears[nextGear].Ratio * outAngularVelocity;

				if (!IsBelowDownShiftCurve(nextGear, response.ClutchPowerRequest / nextAngularVelocity, nextAngularVelocity)) {
					var fullLoadPower = response.EnginePowerRequest - response.DeltaFullLoad;
					var reserve = 1 - (response.EnginePowerRequest / fullLoadPower).Cast<Scalar>();

					if (reserve >= Data.TorqueReserve) {
						NextGear = nextGear;
						return true;
					}
				}
			}

			return false;
		}

		public uint NextGear { get; set; }
	}

	//TODO Implement MTShiftStrategy
	public class MTShiftStrategy : ShiftStrategy
	{
		public MTShiftStrategy(GearboxData data, IDataBus bus) : base(data, bus) {}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque,
			PerSecond inAngularSpeed, uint gear, Second lastShiftTime)
		{
			throw new NotImplementedException();
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}
	}

	// TODO Implement ATShiftStrategy
	public class ATShiftStrategy : ShiftStrategy
	{
		public ATShiftStrategy(GearboxData data, IDataBus bus) : base(data, bus) {}

		public override uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}

		public override bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque,
			PerSecond inAngularSpeed, uint gear, Second lastShiftTime)
		{
			throw new NotImplementedException();
		}

		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new NotImplementedException();
		}
	}

	// TODO Implement CustomShiftStrategy
	public class CustomShiftStrategy : ShiftStrategy
	{
		public CustomShiftStrategy(GearboxData data, IDataBus dataBus) : base(data, dataBus) {}

		public override bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque,
			PerSecond inAngularSpeed, uint gear, Second lastShiftTime)
		{
			throw new NotImplementedException();
		}

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
	}
}
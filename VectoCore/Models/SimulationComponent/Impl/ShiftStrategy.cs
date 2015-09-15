using System;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	// TODO:
	// * EarlyUpshift (shift before outside of up-shift curve if outTorque reserve for the next higher gear is fullfilled)

	public abstract class ShiftStrategy : IShiftStrategy
	{
		public abstract uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		public abstract uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		protected GearboxData Data;
		public Gearbox Gearbox { get; set; }


		protected ShiftStrategy(GearboxData data)
		{
			Data = data;
		}

		/// <summary>
		/// Tests if the gearbox should shift down.
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
		/// Tests if the gearbox should shift up.
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
		protected static bool IsOnLeftSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).IsGreater(0);
		}

		/// <summary>
		/// Tests if current power request is left or right of the shiftpolygon segment
		/// </summary>
		protected static bool IsOnRightSide(PerSecond angularSpeed, NewtonMeter torque, ShiftPolygon.ShiftPolygonEntry from,
			ShiftPolygon.ShiftPolygonEntry to)
		{
			var ab = new { X = to.AngularSpeed - from.AngularSpeed, Y = to.Torque - from.Torque };
			var ac = new { X = angularSpeed - from.AngularSpeed, Y = torque - from.Torque };
			return (ab.X * ac.Y - ab.Y * ac.X).IsSmaller(0);
		}
	}

	public class AMTShiftStrategy : ShiftStrategy
	{
		protected uint PreviousGear;

		public AMTShiftStrategy(GearboxData data) : base(data)
		{
			PreviousGear = 1;
		}

		private uint GetGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed, bool skipGears,
			double torqueReserve)
		{
			var maxGear = (uint)(skipGears ? Data.Gears.Count : Math.Min(PreviousGear + 1, Data.Gears.Count));
			var minGear = skipGears ? 1 : Math.Max(PreviousGear - 1, 1);

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
			PreviousGear = GetGear(absTime, dt, outTorque, outEngineSpeed, Data.SkipGears, Data.TorqueReserve);
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			return IsBelowDownShiftCurve(gear, torque, angularSpeed) || IsAboveUpShiftCurve(gear, torque, angularSpeed);
		}


		public override uint InitGear(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			return GetGear(absTime, dt, outTorque, outEngineSpeed, true, Data.StartTorqueReserve);
		}
	}

	//TODO Implementd MTShiftStrategy
	public class MTShiftStrategy : ShiftStrategy
	{
		public MTShiftStrategy(GearboxData data) : base(data) {}

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
		public ATShiftStrategy(GearboxData data) : base(data) {}

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
}
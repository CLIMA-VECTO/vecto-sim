using TUGraz.VectoCore.Configuration;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Impl
{
	public abstract class ShiftStrategy : IShiftStrategy
	{
		public abstract uint Engage(NewtonMeter torque, PerSecond angularSpeed);
		public abstract void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		public abstract bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		public abstract uint InitGear(NewtonMeter torque, PerSecond angularSpeed);

		protected GearboxData Data;
		public Gearbox Gearbox { get; set; }


		protected ShiftStrategy(GearboxData data)
		{
			Data = data;
		}

		/// <summary>
		/// Tests if the gearbox should shift down.
		/// </summary>
		protected bool ShouldShiftDown(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
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
		protected bool ShouldShiftUp(uint gear, NewtonMeter inTorque, PerSecond inEngineSpeed)
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
		protected uint LastGear;

		public AMTShiftStrategy(GearboxData data)
			: base(data)
		{
			LastGear = 1;
		}

		public uint GetGear(uint gear, NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			var loopCount = Data.Gears.Count; // protection against infinite loops
			do {
				var inEngineSpeed = outEngineSpeed * Data.Gears[gear].Ratio;
				var inTorque = Data.Gears[gear].LossMap.GearboxInTorque(inEngineSpeed, outTorque);
				if (ShouldShiftUp(gear, inTorque, inEngineSpeed)) {
					gear++;
					continue;
				}

				if (ShouldShiftDown(gear, inTorque, inEngineSpeed)) {
					gear--;
					continue;
				}
				break;
			} while (Data.SkipGears && loopCount-- > 0);

			if (gear == 0) {
				throw new VectoSimulationException("Could not find gear! outTorque: {0}, outEngineSpeed: {1}, skipGears: {2}",
					outTorque, outEngineSpeed, Data.SkipGears);
			}
			return gear;
		}

		public override uint Engage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			return GetGear(LastGear, outTorque, outEngineSpeed);
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			LastGear = GetGear(Gearbox.Gear, outTorque, outEngineSpeed);
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			return ShouldShiftDown(gear, torque, angularSpeed) || ShouldShiftUp(gear, torque, angularSpeed);
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			return Engage(torque, angularSpeed);
		}
	}

	//TODO Implementd MTShiftStrategy
	public class MTShiftStrategy : ShiftStrategy
	{
		public MTShiftStrategy(GearboxData data) : base(data) {}

		public override uint Engage(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}
	}

	// TODO Implement ATShiftStrategy
	public class ATShiftStrategy : ShiftStrategy
	{
		public ATShiftStrategy(GearboxData data) : base(data) {}

		public override uint Engage(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}

		public override uint InitGear(NewtonMeter torque, PerSecond angularSpeed)
		{
			throw new System.NotImplementedException();
		}
	}
}
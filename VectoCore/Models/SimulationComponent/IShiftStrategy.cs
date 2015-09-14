using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IShiftStrategy
	{
		bool ShiftRequired(uint gear, NewtonMeter torque, PerSecond angularSpeed);
		uint InitGear(NewtonMeter torque, PerSecond angularSpeed);
		uint Engage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		void Disengage(NewtonMeter outTorque, PerSecond outEngineSpeed);
		Gearbox Gearbox { get; set; }
	}
}
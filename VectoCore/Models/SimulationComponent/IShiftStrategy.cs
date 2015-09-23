using TUGraz.VectoCore.Models.SimulationComponent.Impl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	/// <summary>
	/// Interface for the ShiftStrategy. Decides when to shift and which gear to take.
	/// </summary>
	public interface IShiftStrategy
	{
		/// <summary>
		/// Checks if a shift operation is required.
		/// </summary>
		bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque, PerSecond inAngularVelocity, uint gear, Second lastShiftTime);

		/// <summary>
		/// Returns an appropriate starting gear after a vehicle standstill.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="outEngineSpeed">The angular speed.</param>
		/// <returns></returns>
		uint InitGear(Second absTime, Second dt, NewtonMeter torque, PerSecond outEngineSpeed);

		/// <summary>
		/// Engages a gear.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outEngineSpeed">The out engine speed.</param>
		/// <returns></returns>
		uint Engage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		/// <summary>
		/// Disengages a gear.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outEngineSpeed">The out engine speed.</param>
		void Disengage(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outEngineSpeed);

		/// <summary>
		/// Gets or sets the gearbox.
		/// </summary>
		/// <value>
		/// The gearbox.
		/// </value>
		Gearbox Gearbox { get; set; }
	}
}
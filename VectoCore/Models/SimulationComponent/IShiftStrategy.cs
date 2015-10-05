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
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outAngularVelocity">The out angular velocity.</param>
		/// <param name="inTorque">The in torque.</param>
		/// <param name="inAngularVelocity">The in angular velocity.</param>
		/// <param name="gear">The current gear.</param>
		/// <param name="lastShiftTime">The last shift time.</param>
		/// <returns><c>true</c> if a shift is required, <c>false</c> otherwise.</returns>
		bool ShiftRequired(Second absTime, Second dt, NewtonMeter outTorque, PerSecond outAngularVelocity,
			NewtonMeter inTorque, PerSecond inAngularVelocity, uint gear, Second lastShiftTime);

		/// <summary>
		/// Returns an appropriate starting gear after a vehicle standstill.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="torque">The torque.</param>
		/// <param name="outAngularVelocity">The angular speed.</param>
		/// <returns>The initial gear.</returns>
		uint InitGear(Second absTime, Second dt, NewtonMeter torque, PerSecond outAngularVelocity);

		/// <summary>
		/// Engages a gear.
		/// </summary>
		/// <param name="absTime">The abs time.</param>
		/// <param name="dt">The dt.</param>
		/// <param name="outTorque">The out torque.</param>
		/// <param name="outEngineSpeed">The out engine speed.</param>
		/// <returns>The gear to take.</returns>
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
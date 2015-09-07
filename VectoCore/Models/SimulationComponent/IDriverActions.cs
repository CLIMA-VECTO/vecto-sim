using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.DataBus;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent
{
	public interface IDriverActions
	{
		/// <summary>
		/// perform an 'acceleration driving action', i.e., accelerate the vehicle to the given target velocity but limit the
		/// acceleration by the driver model (acceleration/deceleration curve). The acceleration is adjusted such that the engine 
		/// is not overloaded. Brakes are not activated.
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="targetVelocity"></param>
		/// <param name="gradient"></param>
		/// <param name="previousResponse"></param>
		/// <returns>
		/// * ResponseSuccess
		/// * ResponseUnderload: engine's operating point is below drag curve (vehicle accelerates more than driver model allows; engine's drag load is not sufficient for limited acceleration
		/// * ResponseGearShift: gearbox needs to shift gears, vehicle can not accelerate (traction interruption)
		/// </returns>
		IResponse DrivingActionAccelerate(Second absTime, Meter ds, MeterPerSecond targetVelocity, Radian gradient,
			IResponse previousResponse = null);

		/// <summary>
		/// perform a 'coasting driving action', i.e., the engine is operating at the full drag load. adjust the acceleration such that
		/// this operating point is reached
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="maxVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns>
		/// * ResponseSuccess
		/// * ResponseDrivingCycleDistanceExceeded: vehicle is at low speed, coasting would lead to stop before ds is reached.
		/// * ResponseSpeedLimitExceeded: vehicle accelerates during coasting which would lead to exceeding the given maxVelocity (e.g., driving downhill, engine's drag load is not sufficient)
		/// * ResponseUnderload: engine's operating point is below drag curve (vehicle accelerates more than driver model allows; engine's drag load is not sufficient for limited acceleration
		/// * ResponseGearShift: gearbox needs to shift gears, vehicle can not accelerate (traction interruption)
		/// </returns>
		IResponse DrivingActionCoast(Second absTime, Meter ds, MeterPerSecond maxVelocity, Radian gradient);

		/// <summary>
		/// perform a 'brake driving action', i.e. decelerate the vehicle by using the mechanical brakes to the next target speed
		/// the deceleration is limited by the driver's acceleration/deceleration curve.
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="nextTargetSpeed"></param>
		/// <param name="gradient"></param>
		/// <param name="previousResponse"></param>
		/// <returns>
		/// * ResponseSuccess
		/// * ResponseDrivingCycleDistanceExceeded: vehicle is at low speed, coasting would lead to stop before ds is reached.
		/// </returns>
		IResponse DrivingActionBrake(Second absTime, Meter ds, MeterPerSecond nextTargetSpeed, Radian gradient,
			IResponse previousResponse = null);

		/// <summary>
		/// perform a 'roll driving action', i.e., the clutch is open and the vehicle rolls without motoring. adjust the acceleration 
		/// such that the torque at the gearbox' input (engine side) is zero
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="ds"></param>
		/// <param name="maxVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		IResponse DrivingActionRoll(Second absTime, Meter ds, MeterPerSecond maxVelocity, Radian gradient);


		/// <summary>
		/// perform a 'halt driving action', i.e., the vehicle stops for the given amount of time. 
		/// </summary>
		/// <param name="absTime"></param>
		/// <param name="dt"></param>
		/// <param name="targetVelocity"></param>
		/// <param name="gradient"></param>
		/// <returns></returns>
		IResponse DrivingActionHalt(Second absTime, Second dt, MeterPerSecond targetVelocity, Radian gradient);

		/// <summary>
		/// Compute the distance required to decelerate the vehicle from the current velocity to the given target velocity
		/// considering the driver's acceleration/deceleration curve
		/// </summary>
		/// <param name="targetSpeed"></param>
		/// <returns></returns>
		Meter ComputeDecelerationDistance(MeterPerSecond targetSpeed);

		/// <summary>
		/// access the vehicle's data bus to get information from other components.
		/// </summary>
		IDataBus DataBus { get; }

		MeterPerSquareSecond LookaheadDeceleration { get; }
	}
}
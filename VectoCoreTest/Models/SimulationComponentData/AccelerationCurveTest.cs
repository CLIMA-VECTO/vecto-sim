using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.SimulationComponent.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
	[TestClass]
	public class AccelerationCurveTest
	{
		public const double Tolerance = 0.0001;
		public AccelerationCurveData Data;

		public void EqualAcceleration(double velocity, double acceleration, double deceleration)
		{
			var entry = Data.Lookup(velocity.SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>());
			Assert.AreEqual(entry.Acceleration.Double(), acceleration, Tolerance);
			Assert.AreEqual(entry.Deceleration.Double(), deceleration, Tolerance);
		}

		[TestMethod]
		public void AccelerationTest()
		{
			Data = AccelerationCurveData.ReadFromFile(@"TestData\Components\Coach.vacc");

			// FIXED POINTS
			EqualAcceleration(0, 1.01570922360353, -0.231742702878269);
			EqualAcceleration(5, 1.38546581120225, -0.45346198022574);
			EqualAcceleration(10, 1.34993329755465, -0.565404125020508);
			EqualAcceleration(15, 1.29026714002479, -0.703434814668512);
			EqualAcceleration(20, 1.16369598822194, -0.677703399378421);
			EqualAcceleration(25, 1.04024417156355, -0.63631961226452);
			EqualAcceleration(30, 0.910278494884728, -0.548894523516266);
			EqualAcceleration(35, 0.785875078338323, -0.453995336940216);
			EqualAcceleration(40, 0.69560012996407, -0.385460695652016);
			EqualAcceleration(45, 0.648984223443223, -0.349181329186105);
			EqualAcceleration(50, 0.594249623931624, -0.309125096967231);
			EqualAcceleration(55, 0.559156929181929, -0.296716093796643);
			EqualAcceleration(60, 0.541508805860806, -0.270229542673924);
			EqualAcceleration(65, 0.539582904761905, -0.256408113084341);
			EqualAcceleration(70, 0.539103523809524, -0.217808535739946);
			EqualAcceleration(75, 0.529581598997494, -0.18609307386602);
			EqualAcceleration(80, 0.496418462064251, -0.142683384645006);
			EqualAcceleration(85, 0.453932619248656, -0.117950211164234);
			EqualAcceleration(90, 0.397824554210839, -0.102997621205622);
			EqualAcceleration(95, 0.33969661577071, -0.102997621205622);
			EqualAcceleration(100, 0.289428370365158, -0.102997621205622);
			EqualAcceleration(105, 0.256471472751248, -0.102997621205622);
			EqualAcceleration(110, 0.24, -0.102997621205622);
			EqualAcceleration(115, 0.22, -0.102997621205622);
			EqualAcceleration(120, 0.2, -0.102997621205622);

			// INTERPOLATED POINTS
			EqualAcceleration(0, 1.015709224, -0.231742703);
			EqualAcceleration(2.5, 1.200587517, -0.342602342);
			EqualAcceleration(7.5, 1.367699554, -0.509433053);
			EqualAcceleration(12.5, 1.320100219, -0.63441947);
			EqualAcceleration(17.5, 1.226981564, -0.690569107);
			EqualAcceleration(22.5, 1.10197008, -0.657011506);
			EqualAcceleration(27.5, 0.975261333, -0.592607068);
			EqualAcceleration(32.5, 0.848076787, -0.50144493);
			EqualAcceleration(37.5, 0.740737604, -0.419728016);
			EqualAcceleration(42.5, 0.672292177, -0.367321012);
			EqualAcceleration(47.5, 0.621616924, -0.329153213);
			EqualAcceleration(52.5, 0.576703277, -0.302920595);
			EqualAcceleration(57.5, 0.550332868, -0.283472818);
			EqualAcceleration(62.5, 0.540545855, -0.263318828);
			EqualAcceleration(67.5, 0.539343214, -0.237108324);
			EqualAcceleration(72.5, 0.534342561, -0.201950805);
			EqualAcceleration(77.5, 0.513000031, -0.164388229);
			EqualAcceleration(82.5, 0.475175541, -0.130316798);
			EqualAcceleration(87.5, 0.425878587, -0.110473916);
			EqualAcceleration(92.5, 0.368760585, -0.102997621);
			EqualAcceleration(97.5, 0.314562493, -0.102997621);
			EqualAcceleration(102.5, 0.272949922, -0.102997621);
			EqualAcceleration(107.5, 0.248235736, -0.102997621);
			EqualAcceleration(112.5, 0.23, -0.102997621);
			EqualAcceleration(117.5, 0.21, -0.102997621);

			// EXTRAPOLATE 
			EqualAcceleration(130, 0.16, -0.103);
		}
	}
}
namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox
{
	public class GearData
	{
		public ShiftPolygon ShiftPolygon { get; internal set; }

		public TransmissionLossMap LossMap { get; internal set; }

		public FullLoadCurve FullLoadCurve { get; internal set; }

		public double Ratio { get; internal set; }

		public bool TorqueConverterActive { get; internal set; } // TODO: think about refactoring...

		// public double AverageEfficiency { get; internal set; }
	}
}
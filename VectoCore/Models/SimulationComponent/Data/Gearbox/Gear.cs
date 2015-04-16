namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox
{
	public class Gear
	{
		public ShiftPolygon ShiftPolygon { get; set; }

		public TransmissionLossMap LossMap { get; set; }

		public double Ratio { get; set; }

		public bool TorqueConverterActive { get; set; } // TODO: think about refactoring...
	}
}
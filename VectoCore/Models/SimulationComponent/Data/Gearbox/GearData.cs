namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox
{
	public class GearData
	{
		public ShiftPolygon ShiftPolygon { get; protected set; }

		public TransmissionLossMap LossMap { get; protected set; }

		public double Ratio { get; protected set; }

		public bool TorqueConverterActive { get; protected set; } // TODO: think about refactoring...

		public double AverageEfficiency { get; set; }

		public GearData(TransmissionLossMap lossMap, ShiftPolygon shiftPolygon, double ratio,
			bool torqueconverterActive)
		{
			LossMap = lossMap;
			ShiftPolygon = shiftPolygon;
			Ratio = ratio;
			TorqueConverterActive = torqueconverterActive;
		}
	}
}
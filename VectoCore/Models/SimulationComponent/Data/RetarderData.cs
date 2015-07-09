using System.IO;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class RetarderData : SimulationComponentData
	{
		public enum RetarderType
		{
			None,
			Primary,
			Secondary
		}

		public RetarderLossMap LossMap { get; internal set; }

		public RetarderType Type { get; internal set; }

		public double Ratio { get; internal set; }
	}
}
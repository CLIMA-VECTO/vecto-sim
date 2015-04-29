using System.Runtime.InteropServices;
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

		private readonly Data _data;

		private readonly RetarderType _type;

		public RetarderData(Data data)
		{
			_data = data;
			LossMap = RetarderLossMap.ReadFromFile(_data.File);
			switch (_data.TypeStr) {
				case "Primary":
					_type = RetarderType.Primary;
					break;
				case "Secondary":
					_type = RetarderType.Secondary;
					break;
				default:
					_type = RetarderType.None;
					break;
			}
		}

		public RetarderLossMap LossMap { get; private set; }

		public RetarderType Type
		{
			get { return _type; }
		}

		public double Ratio
		{
			get { return _data.Ratio; }
			set { _data.Ratio = value; }
		}

		public class Data
		{
			[JsonProperty(Required = Required.Always)] public string TypeStr;

			[JsonProperty] public double Ratio;

			[JsonProperty] public string File;
		}
	}
}
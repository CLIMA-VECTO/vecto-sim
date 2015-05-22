using System;
using Common.Logging;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class SimulationComponentData
	{
		[NonSerialized] protected ILog Log;

		public SimulationComponentData()
		{
			Log = LogManager.GetLogger(GetType());
		}

		protected static Tuple<int, bool> GetFileVersion(string jsonStr)
		{
			dynamic json = JsonConvert.DeserializeObject(jsonStr);
			return new Tuple<int, bool>(Int32.Parse(json.Header.FileVersion.ToString()),
				Boolean.Parse(json.Body.SavedInDeclMode.ToString()));
		}
	}
}
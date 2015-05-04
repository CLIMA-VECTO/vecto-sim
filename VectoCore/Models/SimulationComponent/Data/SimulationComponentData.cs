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


		protected static int GetFileVersion(string jsonStr)
		{
			dynamic json = JsonConvert.DeserializeObject(jsonStr);
			return json.Header.FileVersion;
		}
	}
}
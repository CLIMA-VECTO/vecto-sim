using System;
using Common.Logging;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.FileIO
{
	public class InputFileReader
	{
		protected ILog Log;

		protected InputFileReader()
		{
			Log = LogManager.GetLogger(GetType());
		}

		protected Tuple<int, bool> GetFileVersion(string jsonStr)
		{
			dynamic json = JsonConvert.DeserializeObject(jsonStr);
			return new Tuple<int, bool>(Int32.Parse(json.Header.FileVersion.ToString()),
				Boolean.Parse(json.Body.SavedInDeclMode.ToString()));
		}
	}
}
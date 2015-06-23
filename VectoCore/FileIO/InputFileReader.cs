using System;
using System.CodeDom;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.SimulationComponent.Data;

namespace TUGraz.VectoCore.FileIO
{

	public class InputFileReader
	{
		protected ILog Log;

		protected class VersionInfo
		{
			public bool SavedInDeclarationMode;
			public int Version;
		}

		protected InputFileReader()
		{
			Log = LogManager.GetLogger(GetType());
		}

		protected VersionInfo GetFileVersion(string jsonStr)
		{
			var data = new { Header = new { FileVersion = -1 }, Body = new { SavedInDeclMode = false } };
			data = JsonConvert.DeserializeAnonymousType(jsonStr, data);
			return new VersionInfo { SavedInDeclarationMode = data.Body.SavedInDeclMode, Version = data.Header.FileVersion };
		}

		protected T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
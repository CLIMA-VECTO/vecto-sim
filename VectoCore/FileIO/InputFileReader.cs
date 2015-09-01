using Newtonsoft.Json;
using TUGraz.VectoCore.Models;

namespace TUGraz.VectoCore.FileIO
{
	public class InputFileReader : LoggingObject
	{
		protected class VersionInfo
		{
			public bool SavedInDeclarationMode;
			public int Version;
		}

		protected InputFileReader() {}

		protected static VersionInfo GetFileVersion(string jsonStr)
		{
			var data = new { Header = new { FileVersion = -1 }, Body = new { SavedInDeclMode = false } };
			data = JsonConvert.DeserializeAnonymousType(jsonStr, data);
			return new VersionInfo { SavedInDeclarationMode = data.Body.SavedInDeclMode, Version = data.Header.FileVersion };
		}
	}
}
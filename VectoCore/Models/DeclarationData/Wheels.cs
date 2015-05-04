using System.IO;
using System.Security.AccessControl;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.DeclarationData
{
	public class Wheels
	{
		private static Wheels _instance;

		private const string ResourceId = "DeclarationWheels";

		protected Wheels()
		{
			System.Reflection.Assembly myAssembly;
			//myAssembly = this.GetType().Assembly;
			myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
			//string[] resources = myAssembly.GetManifestResourceNames();
			//string list = "";

			//foreach (string resource in resources)
			//	{
			//	list += resource + "\r\n";
			//	}
			//System.Resources.ResourceManager myManager = new System.Resources.ResourceManager("Resources.Wheels", myAssembly);

			System.IO.Stream file =
				myAssembly.GetManifestResourceStream("TUGraz.VectoCore.Properties.Resources.resources");

			//var csvFile = VectoCSVFile.ReadStream(file);
			var reader = new StreamReader(file);
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();
			}
			//var test = myManager.GetObject(ResourceId);
		}

		public static Wheels Instance()
		{
			if (_instance == null) {
				_instance = new Wheels();
			}
			return _instance;
		}
	}
}
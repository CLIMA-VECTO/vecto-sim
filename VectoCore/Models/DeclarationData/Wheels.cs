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
			string[] resources = myAssembly.GetManifestResourceNames();
			string list = "";

			foreach (string resource in resources) {
				list += resource + "\r\n";
			}
			//System.Resources.ResourceManager myManager = new System.Resources.ResourceManager("TUGraz.VectoCore.Resources.Declaration.Wheels.csv", myAssembly);

			System.IO.Stream file =
				myAssembly.GetManifestResourceStream("TUGraz.VectoCore.Resources.Declaration.Wheels.csv");

			var csvFile = VectoCSVFile.ReadStream(file);
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
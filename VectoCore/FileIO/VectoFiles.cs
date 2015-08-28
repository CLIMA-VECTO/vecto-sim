using System.IO;
using System.Runtime.Serialization;

namespace TUGraz.VectoCore.FileIO
{
	public abstract class VectoBaseFile
	{
		protected string basePath;

		[DataMember]
		internal string BasePath
		{
			get { return basePath; }
			set { basePath = Path.GetDirectoryName(Path.GetFullPath(value)); }
		}
	}


	public abstract class VectoJobFile : VectoBaseFile
	{
		private string _jobFile;

		[DataMember]
		internal string JobFile
		{
			get { return _jobFile; }
			set { _jobFile = Path.GetFileName(value); }
		}
	}

	public abstract class VectoVehicleFile : VectoBaseFile {}

	public abstract class VectoGearboxFile : VectoBaseFile {}

	public abstract class VectoEngineFile : VectoBaseFile {}

	public abstract class VectoAuxiliaryFile : VectoBaseFile {}
}
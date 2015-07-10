using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace TUGraz.VectoCore.FileIO
{
	public abstract class VectoJobFile
	{
		[DataMember] internal string BasePath;
		[DataMember] internal string JobFile;
	}

	public abstract class VectoVehicleFile
	{
		[DataMember] internal string BasePath;
	}

	public abstract class VectoGearboxFile
	{
		[DataMember] internal string BasePath;
	}

	public abstract class VectoEngineFile
	{
		[DataMember] internal string BasePath;
	}
}
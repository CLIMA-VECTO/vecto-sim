using Newtonsoft.Json;
using TUGraz.VectoCore.FileIO.DeclarationFile;

namespace TUGraz.VectoCore.FileIO.EngineeringFile
{
	internal class EngineFileV2Engineering : EngineFileV2Declaration
	{
		[JsonProperty(Required = Required.Always)] public new DataBodyEng Body;

		public class DataBodyEng : DataBodyDecl
		{
			/// <summary>
			///     [kgm^2] Inertia including Flywheel
			///     Inertia for rotating parts including engine flywheel.
			///     In Declaration Mode the inertia is calculated automatically.
			/// </summary>
			[JsonProperty(Required = Required.Always)] public double Inertia;
		}
	}
}
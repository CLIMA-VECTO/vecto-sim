using System.IO;
using System.Reflection;

namespace TUGraz.VectoCore.Utils
{
	public static class RessourceHelper
	{
		public static Stream ReadStream(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream(resourceName);
		}
	}
}
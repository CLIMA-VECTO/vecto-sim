using System.IO;
using System.Reflection;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public static class RessourceHelper
	{
		public const string Namespace = "TUGraz.VectoCore.Resources.Declaration.";

		public static Stream ReadStream(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resource = assembly.GetManifestResourceStream(resourceName);
			if (resource == null) {
				throw new VectoException("Resource file not found: " + resourceName);
			}
			return resource;
		}
	}
}
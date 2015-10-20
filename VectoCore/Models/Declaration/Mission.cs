using System.Collections.Generic;
using System.IO;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public enum LoadingType
	{
		FullLoading,
		ReferenceLoad,
		EmptyLoading,
	}

	public static class LoadingTypeHelper
	{
		public static string GetShortName(this LoadingType loadingType)
		{
			return loadingType.ToString().Substring(0, 1);
		}
	}


	public class Mission
	{
		public MissionType MissionType { get; set; }
		public string CrossWindCorrection { get; set; }
		public double[] AxleWeightDistribution { get; set; }
		public double[] TrailerAxleWeightDistribution { get; set; }

		public Kilogram MassExtra { get; set; }

		public Kilogram MinLoad { get; set; }
		public Kilogram RefLoad { get; set; }
		public Kilogram MaxLoad { get; set; }

		public Dictionary<LoadingType, Kilogram> Loadings
		{
			get
			{
				return new Dictionary<LoadingType, Kilogram> {
					{ LoadingType.EmptyLoading, MinLoad },
					{ LoadingType.ReferenceLoad, RefLoad },
					{ LoadingType.FullLoading, MaxLoad }
				};
			}
		}

		public Stream CycleFile { get; set; }

		public bool UseCdA2 { get; set; }

		public class LoadingEntry
		{
			public Kilogram LoadingWeight;
			public string Name;
		}
	}
}
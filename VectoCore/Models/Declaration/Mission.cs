using System.Collections.Generic;
using System.IO;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
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

		public IEnumerable<LoadingEntry> Loadings
		{
			get
			{
				return new[] {
					new LoadingEntry() { LoadingWeight = MinLoad, Name = "EmptyLoading" },
					new LoadingEntry() { LoadingWeight = RefLoad, Name = "ReferenceLoad" },
					new LoadingEntry() { LoadingWeight = MaxLoad, Name = "FullLoading" }
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
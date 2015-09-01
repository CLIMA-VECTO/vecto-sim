using System.Diagnostics.CodeAnalysis;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum GearboxType
	{
		MT, // Manual Transmission
		AMT, // Automated Manual Transmission
		AT, // Automatic Transmission
		Custom
	}

	public static class GearBoxTypeExtension
	{
		public static bool EarlyShiftGears(this GearboxType type)
		{
			switch (type) {
				case GearboxType.MT:
					return false;
				case GearboxType.AMT:
					return true;
				case GearboxType.AT:
					return false;
			}
			return false;
		}

		public static bool SkipGears(this GearboxType type)
		{
			switch (type) {
				case GearboxType.MT:
					return true;
				case GearboxType.AMT:
					return true;
				case GearboxType.AT:
					return false;
			}
			return false;
		}

		public static Second TractionInterruption(this GearboxType type)
		{
			switch (type) {
				case GearboxType.MT:
					return 2.SI<Second>();
				case GearboxType.AMT:
					return 1.SI<Second>();
				case GearboxType.AT:
					return 0.8.SI<Second>();
			}
			return 0.SI<Second>();
		}
	}
}
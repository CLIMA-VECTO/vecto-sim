using System;
using NLog;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public enum CrossWindCorrectionMode
	{
		NoCorrection,
		SpeedDependentCorrectionFactor,
		VAirBetaLookupTable,
		DeclarationModeCorrection
	}

	public static class CrossWindCorrectionModeHelper
	{
		public static CrossWindCorrectionMode Parse(string correctionMode)
		{
			if (correctionMode.Equals("CdofVEng", StringComparison.OrdinalIgnoreCase)) {
				return CrossWindCorrectionMode.SpeedDependentCorrectionFactor;
			}
			if (correctionMode.Equals("CdofVdecl", StringComparison.OrdinalIgnoreCase)) {
				return CrossWindCorrectionMode.DeclarationModeCorrection;
			}
			if (correctionMode.Equals("CdofBeta", StringComparison.OrdinalIgnoreCase)) {
				return CrossWindCorrectionMode.VAirBetaLookupTable;
			}
			if (correctionMode.Equals("Off", StringComparison.OrdinalIgnoreCase)) {
				return CrossWindCorrectionMode.NoCorrection;
			}
			LogManager.GetLogger(typeof(CrossWindCorrectionModeHelper).ToString())
				.Warn("Invalid Crosswind correction Mode given. Ignoring Crosswind Correction!");
			return CrossWindCorrectionMode.NoCorrection;
		}
	}
}
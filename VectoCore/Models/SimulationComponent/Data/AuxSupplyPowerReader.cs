using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	/// <summary>
	/// Reader for Auxiliary Supply Power. Is used by Distance, Time, and EngineOnly based Data Parser.
	/// </summary>
	public static class AuxSupplyPowerReader
	{
		private const string AuxSupplyPowerField = "Aux_";

		/// <summary>
		/// [W]. Reads Auxiliary Supply Power (defined by Fields.AuxiliarySupplyPower-Prefix).
		/// </summary>
		public static Dictionary<string, Watt> Read(DataRow row)
		{
			var auxCols = row.Table.Columns.Cast<DataColumn>().
				Where(col => col.ColumnName.StartsWith(AuxSupplyPowerField));

			return auxCols.ToDictionary(key => key.ColumnName,
				value => row.ParseDouble(value).SI().Kilo.Watt.Cast<Watt>());
		}
	}
}
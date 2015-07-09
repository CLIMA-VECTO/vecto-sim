using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Rims : LookupData<string, Rims.RimsEntry>
	{
		internal Rims()
		{
			var csvFile = ReadCsvFile(ResourceId);
			ParseData(csvFile);
		}

		protected override string ResourceId
		{
			get { return "TUGraz.VectoCore.Resources.Declaration.Rims.csv"; }
		}

		protected override sealed void ParseData(DataTable table)
		{
			Data = (from DataRow row in table.Rows
				select new RimsEntry {
					RimsType = row[0].ToString(),
					F_a = row.ParseDouble(1),
					F_b = row.ParseDouble(2)
				}).ToDictionary(e => e.RimsType);
		}

		public class RimsEntry
		{
			public string RimsType;
			public double F_a;
			public double F_b;
		}
	}
}
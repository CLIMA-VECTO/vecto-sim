using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class DeclarationRims : LookupData<string, DeclarationRims.RimsEntry>
	{
		protected string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Rims.csv";

		public DeclarationRims()
		{
			ParseData(ReadCsvResource(ResourceId));
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
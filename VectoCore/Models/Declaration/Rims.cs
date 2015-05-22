﻿using System;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Resources.Declaration;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class Rims : LookupData<Rims.RimsEntry>
	{
		private const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.Rims.csv";

		internal Rims()
		{
			var csvFile = ReadCsvFile(ResourceId);
			ParseData(csvFile);
		}


		protected override sealed void ParseData(DataTable table)
		{
			_data = (from DataRow row in table.Rows
				select new RimsEntry {
					RimsType = row[0].ToString(),
					F_a = row.ParseDouble(1),
					F_b = row.ParseDouble(2),
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
using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Resources.Declaration
{
	public abstract class LookupData<TEntryType>
	{
		protected Dictionary<String, TEntryType> _data;


		protected DataTable ReadCsvFile(string resourceId)
		{
			var myAssembly = System.Reflection.Assembly.GetExecutingAssembly();

			var file = myAssembly.GetManifestResourceStream(resourceId);

			return VectoCSVFile.ReadStream(file);
		}

		protected abstract void ParseData(DataTable table);

		public TEntryType Lookup(String key)
		{
			var retVal = default(TEntryType);
			if (_data.ContainsKey(key)) {
				retVal = _data[key];
			}
			return retVal;
		}
	}
}
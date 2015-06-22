using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Common.Logging;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public abstract class LookupData<TKeyType, TEntryType>
	{
		protected LookupData()
		{
			Log = LogManager.GetLogger(GetType());
			var csvFile = ReadCsvFile(ResourceId);
			ParseData(csvFile);
		}

		[NonSerialized] protected ILog Log;

		protected abstract string ResourceId { get; }
		protected abstract void ParseData(DataTable table);

		protected Dictionary<TKeyType, TEntryType> Data;

		protected DataTable ReadCsvFile(string resourceId)
		{
			var myAssembly = Assembly.GetExecutingAssembly();
			var file = myAssembly.GetManifestResourceStream(resourceId);
			return VectoCSVFile.ReadStream(file);
		}

		public virtual TEntryType Lookup(TKeyType key)
		{
			var retVal = default(TEntryType);
			if (Data.ContainsKey(key)) {
				retVal = Data[key];
			}
			return retVal;
		}
	}

	public abstract class LookupData<TKey1, TKey2, TValue>
	{
		[NonSerialized] protected ILog Log;
		protected abstract string ResourceId { get; }
		protected abstract void ParseData(DataTable table);
		public abstract TValue Lookup(TKey1 key1, TKey2 key2);

		protected LookupData()
		{
			Log = LogManager.GetLogger(GetType());
			var csvFile = ReadCsvFile(ResourceId);
			ParseData(csvFile);
		}

		protected DataTable ReadCsvFile(string resourceId)
		{
			var myAssembly = Assembly.GetExecutingAssembly();
			var file = myAssembly.GetManifestResourceStream(resourceId);
			return VectoCSVFile.ReadStream(file);
		}
	}
}
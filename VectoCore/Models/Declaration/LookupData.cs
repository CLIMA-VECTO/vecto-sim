﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Common.Logging;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public abstract class LookupData
	{
		protected LookupData()
		{
			Log = LogManager.GetLogger(GetType());
			//var csvFile = ReadCsvResource(ResourceId);
			//ParseData(csvFile);
			//ParseData(ReadData());
		}

		[NonSerialized] protected ILog Log;

		//protected abstract string ResourceId { get; }

		//protected abstract DataTable ReadData();
		protected abstract void ParseData(DataTable table);

		protected DataTable ReadCsvResource(string resourceId)
		{
			var myAssembly = Assembly.GetExecutingAssembly();
			var file = myAssembly.GetManifestResourceStream(resourceId);
			return VectoCSVFile.ReadStream(file);
		}

		protected DataTable ReadCsvFile(string fileName)
		{
			return VectoCSVFile.Read(fileName);
		}
	}


	public abstract class LookupData<TKey, TValue> : LookupData
	{
		protected Dictionary<TKey, TValue> Data;

		public virtual TValue Lookup(TKey key)
		{
			var retVal = default(TValue);
			if (Data.ContainsKey(key)) {
				retVal = Data[key];
			}
			return retVal;
		}
	}

	public abstract class LookupData<TKey1, TKey2, TValue> : LookupData
	{
		public abstract TValue Lookup(TKey1 key1, TKey2 key2);
	}

	public abstract class LookupData<TKey1, TKey2, TKey3, TValue> : LookupData
	{
		public abstract TValue Lookup(TKey1 key1, TKey2 key2, TKey3 key3);
	}

	public abstract class LookupData<TKey1, TKey2, TKey3, TKey4, TValue> : LookupData
	{
		public abstract TValue Lookup(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4);
	}
}
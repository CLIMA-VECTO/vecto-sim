using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
	public static class DataTableExtensionMethods
	{
		public static double ParseDoubleOrGetDefault(this DataRow row, string columnName,
			double defaultValue = default(double))
		{
			if (row.Table.Columns.Contains(columnName)) {
				double result;
				if (double.TryParse(row.Field<string>(columnName), NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
					return result;
				}
			}
			return defaultValue;
		}

		public static double ParseDouble(this DataRow row, int columnIndex)
		{
			return row.ParseDouble(row.Table.Columns[columnIndex]);
		}

		public static double ParseDouble(this DataRow row, string columnName)
		{
			if (!row.Table.Columns.Contains(columnName)) {
				throw new KeyNotFoundException(string.Format("Column {0} was not found in DataRow.", columnName));
			}
			return row.ParseDouble(row.Table.Columns[columnName]);
		}

		public static double ParseDouble(this DataRow row, DataColumn column)
		{
			try {
				return row.Field<string>(column).ToDouble();
			} catch (IndexOutOfRangeException e) {
				throw new VectoException(string.Format("Field {0} was not found in DataRow.", column), e);
			} catch (NullReferenceException e) {
				throw new VectoException(string.Format("Field {0} must not be null.", column), e);
			} catch (FormatException e) {
				throw new VectoException(string.Format("Field {0} is not in a valid number format: {1}", column,
					row.Field<string>(column)), e);
			} catch (OverflowException e) {
				throw new VectoException(string.Format("Field {0} has a value too high or too low: {1}", column,
					row.Field<string>(column)), e);
			} catch (ArgumentNullException e) {
				throw new VectoException(string.Format("Field {0} contains null which cannot be converted to a number.", column), e);
			} catch (Exception e) {
				throw new VectoException(string.Format("Field {0}: {1}", column, e.Message), e);
			}
		}

		public static IEnumerable<T> Values<T>(this DataColumn column)
		{
			return column.Table.AsEnumerable().Select(r => r.Field<T>(column));
		}
	}
}
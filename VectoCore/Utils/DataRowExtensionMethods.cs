using System;
using System.Data;
using System.Globalization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    public static class DataRowExtensionMethods
    {
        public static double GetDouble(this DataRow row, DataColumn column, CultureInfo culture = null)
        {
            //todo ArgumentNullException?
            try
            {
                return double.Parse(row.Field<string>(column), CultureInfo.InvariantCulture);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new VectoException(string.Format("Column {0} was not found in DataRow.", column.ColumnName), e);
            }
            catch (NullReferenceException e)
            {
                throw new VectoException(string.Format("Column {0} must not be null.", column.ColumnName), e);
            }
            catch (FormatException e)
            {
                throw new VectoException(string.Format("Column {0} is not in a valid number format: {1}", column.ColumnName,
                    row.Field<string>(column)), e);
            }
            catch (OverflowException e)
            {
                throw new VectoException(string.Format("Column {0} has a value too high or too low: {1}", column.ColumnName,
                    row.Field<string>(column)), e);
            }
            catch (ArgumentNullException e)
            {
                throw new VectoException(string.Format("Column {0} contains null which cannot be converted to a number.", column.ColumnName), e);
            }
            catch (Exception e)
            {
                throw new VectoException(string.Format("Column {0}: {1}", column.ColumnName, e.Message), e);
            }
        }

        public static double GetDoubleOrDefault(this DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) ? row.GetDouble(columnName) : default(double);
        }

        public static double GetDouble(this DataRow row, string columnName, CultureInfo culture = null)
        {
            //todo ArgumentNullException?
            try
            {
                return double.Parse(row.Field<string>(columnName), CultureInfo.InvariantCulture);
            }
            catch (IndexOutOfRangeException e)
            {
                throw new VectoException(string.Format("Field {0} was not found in DataRow.", columnName), e);
            }
            catch (NullReferenceException e)
            {
                throw new VectoException(string.Format("Field {0} must not be null.", columnName), e);
            }
            catch (FormatException e)
            {
                throw new VectoException(string.Format("Field {0} is not in a valid number format: {1}", columnName,
                    row.Field<string>(columnName)), e);
            }
            catch (OverflowException e)
            {
                throw new VectoException(string.Format("Field {0} has a value too high or too low: {1}", columnName,
                    row.Field<string>(columnName)), e);
            }
            catch (ArgumentNullException e)
            {
                throw new VectoException(string.Format("Field {0} contains null which cannot be converted to a number.", columnName), e);
            }
            catch (Exception e)
            {
                throw new VectoException(string.Format("Field {0}: {1}", columnName, e.Message), e);
            }
        }
    }
}
using System;
using System.Data;
using System.Globalization;
using TUGraz.VectoCore.Exceptions;

namespace TUGraz.VectoCore.Utils
{
    public static class DataRowExtensionMethods
    {
        /// <summary>
        /// Gets the field of a DataRow as double.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The value of the underlying DataRows column field as double.</returns>
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
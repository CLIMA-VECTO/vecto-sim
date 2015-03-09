using System.Data;

namespace TUGraz.VectoCore.Utils
{
    public static class DataRowExtensionMethods
    {
        public static double GetDouble(this DataRow row, string columnName)
        {
            return double.Parse(row.Field<string>(columnName));
        }
    }
}
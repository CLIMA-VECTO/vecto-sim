using System.Collections.Generic;
using System.Data;
using System.IO;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public enum EngineOnlyDrivingCycleFields
    {
        n,
        Pe
    }


    public static class EngineOnlyDrivingCycle
    {
        public static DataTable getDataTable()
        {
            DataTable data = new DataTable();
            data.Columns.Add(EngineOnlyDrivingCycleFields.n.ToString(), typeof(float));
            data.Columns.Add(EngineOnlyDrivingCycleFields.Pe.ToString(), typeof(float));
            return data;
        }

        public static DataTable read(string fileName)
        {
            var data = getDataTable();

            var reader = new StreamReader(fileName);
            // read header line
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                data.Rows.Add(values);
            }
            return data;
        }
    }
}

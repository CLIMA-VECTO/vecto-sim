using System.IO;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
    public class FuelConsumptionMap
    {
        public static FuelConsumptionMap ReadFromFile(string fileName)
        {
            return ReadFromJson(File.ReadAllText(fileName));
        }

        public static FuelConsumptionMap ReadFromJson(string json)
        {
            //todo implement ReadFromJson
            return new FuelConsumptionMap();
        }
    }
}

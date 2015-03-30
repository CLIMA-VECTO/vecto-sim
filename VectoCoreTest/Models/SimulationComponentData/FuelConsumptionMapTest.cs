using System;
using System.IO;
using System.Linq;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Models.SimulationComponentData
{
    [TestClass]
    public class FuelConsumptionMapTest
    {
        private const double Tolerance = 0.0001;

        [TestMethod]
        public void TestFuelConsumption_FixedPoints()
        {
            var map = FuelConsumptionMap.ReadFromFile(@"TestData\Components\24t Coach.vmap");
            var lines = File.ReadAllLines(@"TestData\Components\24t Coach.vmap").Skip(1).ToArray();
            AssertMapValuesEqual(lines, map);
        }

        [TestMethod]
        public void TestFuelConsumption_InterpolatedPoints()
        {
            var map = FuelConsumptionMap.ReadFromFile(@"TestData\Components\24t Coach.vmap");
            var lines = File.ReadAllLines(@"TestData\Components\24t CoachInterpolated.vmap").Skip(1).ToArray();
            AssertMapValuesEqual(lines, map);
        }

        private static void AssertMapValuesEqual(string[] lines, FuelConsumptionMap map)
        {
            for (var i = 1; i < lines.Count(); i++)
            {
                var entry = lines[i].Split(',').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                try
                {
                    Assert.AreEqual((double)entry[2].SI().Gramm.Per.Hour.To().Kilo.Gramm.Per.Second, 
                                    (double)map.GetFuelConsumption(entry[1].SI<NewtonMeter>(), entry[0].RPMtoRad()), 
                                    Tolerance,
                                    string.Format("Line: {0}, n={1}, T={2}", (i + 2), entry[0].SI().Rounds.Per.Minute, entry[1]));

                }
                catch (VectoException ex)
                {
                    throw new VectoException(string.Format("Row {0}: Error in ConsumptionMap n={1}, T={2}: {3}",
                        i + 2, entry[0], entry[1], ex.Message));
                }
            }
        }
    }
}

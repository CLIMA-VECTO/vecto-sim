using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Tests.Reports
{
	[TestClass]
	public class SumWriterTest
	{
		[TestMethod]
		public void TestSumCalcFixedTime()
		{
			var sumWriter = new SummaryFileWriter("testsumcalc_fixed.vsum");

			var modData = new ModalDataWriter("testsumcalc_fixed.vmod");
			modData.AddAuxiliary("FAN");

			for (var i = 0; i < 500; i++) {
				modData[ModalResultField.simulationInterval] = 1.SI<Second>();
				modData[ModalResultField.time] = i.SI<Second>();
				modData[ModalResultField.dist] = i.SI<Meter>();
				modData["FAN"] = 3000.SI<Watt>();
				modData[ModalResultField.Pair] = 3000.SI<Watt>();
				modData[ModalResultField.Proll] = 3000.SI<Watt>();
				modData[ModalResultField.Pgrad] = 3000.SI<Watt>();
				modData[ModalResultField.Paux] = 3000.SI<Watt>();
				modData[ModalResultField.Pbrake] = 3000.SI<Watt>();

				modData[ModalResultField.FCMap] = 1e-4.SI<KilogramPerSecond>();

				modData[ModalResultField.altitude] = 0.SI<Meter>();
				modData.CommitSimulationStep();
			}

			sumWriter.WriteFullPowertrain(modData, "testSumCalc", "--", "--", 0.SI<Kilogram>(), 0.SI<Kilogram>());

			modData.Finish();
			sumWriter.Finish();

			var sumData = VectoCSVFile.Read("testsumcalc_fixed.vsum", false, true);

			// 3kW * 500s => to kWh
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Eair [kWh]"), 1e-3);
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Eaux_FAN [kWh]"), 1e-3);
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Eroll [kWh]"), 1e-3);
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Egrad [kWh]"), 1e-3);
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Eaux [kWh]"), 1e-3);
			Assert.AreEqual(500.0 * 3000.0 / 1000 / 3600, sumData.Rows[0].ParseDouble("Ebrake [kWh]"), 1e-3);

			// 500s * 1e-4 kg/s = 0.05kg  => 0.05kg / 499s => to g/h
			Assert.AreEqual((500.0 * 1e-4) * 1000 * 3600 / 499.0, sumData.Rows[0].ParseDouble("FC-Map [g/h]"), 1e-3);
			// 500s * 1e-4 kg/s = 0.05kg => 0.05kg / 499m => to g/km
			Assert.AreEqual((500.0 * 1e-4) * 1000 * 1000 / 499.0, sumData.Rows[0].ParseDouble("FC-Map [g/km]"), 1e-3);
		}

		[TestMethod]
		public void TestSumCalcVariableTime()
		{
			var sumWriter = new SummaryFileWriter("testsumcalc_var.vsum");

			var modData = new ModalDataWriter("testsumcalc_var.vmod");
			modData.AddAuxiliary("FAN");

			var timeSteps = new[] { 0.5.SI<Second>(), 0.3.SI<Second>(), 1.2.SI<Second>(), 12.SI<Second>(), 0.1.SI<Second>() };
			var powerDemand = new[] { 1000.SI<Watt>(), 1500.SI<Watt>(), 2000.SI<Watt>(), 2500.SI<Watt>(), 3000.SI<Watt>() };

			for (var i = 0; i < 500; i++) {
				modData[ModalResultField.simulationInterval] = timeSteps[i % timeSteps.Count()];
				modData[ModalResultField.time] = i.SI<Second>();
				modData[ModalResultField.dist] = i.SI<Meter>();
				modData["FAN"] = powerDemand[i % powerDemand.Count()];
				modData[ModalResultField.Pair] = powerDemand[i % powerDemand.Count()];
				modData[ModalResultField.Proll] = powerDemand[i % powerDemand.Count()];
				modData[ModalResultField.Pgrad] = powerDemand[i % powerDemand.Count()];
				modData[ModalResultField.Paux] = powerDemand[i % powerDemand.Count()];
				modData[ModalResultField.Pbrake] = powerDemand[i % powerDemand.Count()];

				modData[ModalResultField.altitude] = 0.SI<Meter>();
				modData[ModalResultField.acc] = 0.SI<MeterPerSquareSecond>();
				modData.CommitSimulationStep();
			}

			sumWriter.WriteFullPowertrain(modData, "testSumCalc", "--", "--", 0.SI<Kilogram>(), 0.SI<Kilogram>());

			modData.Finish();
			sumWriter.Finish();

			var sumData = VectoCSVFile.Read("testsumcalc_var.vsum", false, true);

			// sum(dt * p) => to kWh
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Eair [kWh]"), 1e-3);
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Eaux_FAN [kWh]"), 1e-3);
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Eroll [kWh]"), 1e-3);
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Egrad [kWh]"), 1e-3);
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Eaux [kWh]"), 1e-3);
			Assert.AreEqual(0.934722222, sumData.Rows[0].ParseDouble("Ebrake [kWh]"), 1e-3);
		}
	}
}
using System;
using System.Data;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Declaration
{
	public class TorqueConverter : LookupData<double, TorqueConverter.TorqueConverterEntry>
	{
		protected const string ResourceId = "TUGraz.VectoCore.Resources.Declaration.DefaultTC.vtcc";


		public TorqueConverter()
		{
			ParseData(ReadCsvResource(ResourceId));
		}


		[Obsolete("Default Lookup not availabel. Use LookupMu or LookupTorque instead.", true)]
		protected new TorqueConverterEntry Lookup(double key)
		{
			throw new InvalidOperationException(
				"Default Lookup not available. Use TorqueConverter.LookupMu() or TorqueConverter.LookupTorque() instead.");
		}


		public NewtonMeter LookupTorque(double nu, PerSecond angularSpeedIn, PerSecond referenceSpeed)
		{
			var sec = Data.GetSamples(kv => kv.Key < nu);

			if (nu < sec.Item1.Key || sec.Item2.Key < nu) {
				Log.Warn(string.Format("TCextrapol: nu = {0} [n_out/n_in]", nu));
			}

			var torque = VectoMath.Interpolate(sec.Item1.Key, sec.Item2.Key, sec.Item1.Value.Torque, sec.Item2.Value.Torque, nu);
			return torque * Math.Pow((angularSpeedIn / referenceSpeed).Cast<Scalar>(), 2);
		}

		public double LookupMu(double nu)
		{
			var sec = Data.GetSamples(kv => kv.Key < nu);

			if (nu < sec.Item1.Key || sec.Item2.Key < nu) {
				Log.Warn(string.Format("TCextrapol: nu = {0} [n_out/n_in]", nu));
			}

			return VectoMath.Interpolate(sec.Item1.Key, sec.Item2.Key, sec.Item1.Value.Mu, sec.Item2.Value.Mu, nu);
		}


		protected override void ParseData(DataTable table)
		{
			Data.Clear();
			foreach (DataRow row in table.Rows) {
				Data[row.ParseDouble("nue")] = new TorqueConverterEntry {
					Mu = row.ParseDouble("mue"),
					Torque = row.ParseDouble("MP1000 (1000/rpm)^2*Nm").SI<NewtonMeter>()
				};
			}
		}

		public class TorqueConverterEntry
		{
			public double Mu { get; set; }
			public NewtonMeter Torque { get; set; }
		}
	}
}
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class FullLoadCurve : SimulationComponentData
	{
		internal List<FullLoadCurveEntry> FullLoadEntries;
		internal LookupData<PerSecond, Second> PT1Data;

		public static FullLoadCurve ReadFromFile(string fileName, bool declarationMode = false)
		{
			var data = VectoCSVFile.Read(fileName);

			//todo Contract.Requires<VectoException>(data.Columns.Count != 4, "FullLoadCurve Data File must consist of 4 columns.");
			if (data.Columns.Count < 3) {
				throw new VectoException("FullLoadCurve Data File must consist of at least 3 columns.");
			}

			//todo Contract.Requires<VectoException>(data.Rows.Count < 2, "FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			if (data.Rows.Count < 2) {
				throw new VectoException(
					"FullLoadCurve must consist of at least two lines with numeric values (below file header)");
			}

			List<FullLoadCurveEntry> entriesFld;
			if (HeaderIsValid(data.Columns)) {
				entriesFld = CreateFromColumnNames(data);
			} else {
				var log = LogManager.GetLogger<EngineFullLoadCurve>();
				log.WarnFormat(
					"FullLoadCurve: Header Line is not valid. Expected: '{0}, {1}, {2}', Got: '{3}'. Falling back to column index.",
					Fields.EngineSpeed, Fields.TorqueFullLoad,
					Fields.TorqueDrag,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

				entriesFld = CreateFromColumnIndizes(data);
			}

			LookupData<PerSecond, Second> tmp;
			if (declarationMode) {
				tmp = new PT1();
			} else {
				tmp = PT1Curve.ReadFromFile(fileName);
			}

			return new FullLoadCurve { FullLoadEntries = entriesFld, PT1Data = tmp };
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			Contract.Requires(columns != null);
			return columns.Contains(Fields.EngineSpeed)
					&& columns.Contains(Fields.TorqueDrag)
					&& columns.Contains(Fields.TorqueFullLoad);
			//&& columns.Contains(Fields.PT1);
		}

		private static List<FullLoadCurveEntry> CreateFromColumnNames(DataTable data)
		{
			Contract.Requires(data != null);
			return (from DataRow row in data.Rows
				select new FullLoadCurveEntry {
					EngineSpeed = row.ParseDouble(Fields.EngineSpeed).RPMtoRad(),
					TorqueFullLoad = row.ParseDouble(Fields.TorqueFullLoad).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(Fields.TorqueDrag).SI<NewtonMeter>(),
					//PT1 = row.ParseDouble(Fields.PT1).SI<Second>()
				}).ToList();
		}

		private static List<FullLoadCurveEntry> CreateFromColumnIndizes(DataTable data)
		{
			Contract.Requires(data != null);
			return (from DataRow row in data.Rows
				select new FullLoadCurveEntry {
					EngineSpeed = row.ParseDouble(0).RPMtoRad(),
					TorqueFullLoad = row.ParseDouble(1).SI<NewtonMeter>(),
					TorqueDrag = row.ParseDouble(2).SI<NewtonMeter>(),
					//PT1 = row.ParseDouble(3).SI<Second>()
				}).ToList();
		}


		internal class FullLoadCurveEntry
		{
			public PerSecond EngineSpeed { get; set; }

			public NewtonMeter TorqueFullLoad { get; set; }

			public NewtonMeter TorqueDrag { get; set; }

			//public Second PT1 { get; set; }

			#region Equality members

			protected bool Equals(FullLoadCurveEntry other)
			{
				return Equals(EngineSpeed, other.EngineSpeed) && Equals(TorqueFullLoad, other.TorqueFullLoad) &&
						Equals(TorqueDrag, other.TorqueDrag);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) {
					return false;
				}
				if (ReferenceEquals(this, obj)) {
					return true;
				}
				if (obj.GetType() != GetType()) {
					return false;
				}
				return Equals((FullLoadCurveEntry)obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = (EngineSpeed != null ? EngineSpeed.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (TorqueFullLoad != null ? TorqueFullLoad.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (TorqueDrag != null ? TorqueDrag.GetHashCode() : 0);
					return hashCode;
				}
			}

			#endregion
		}

		private static class Fields
		{
			/// <summary>
			///     [rpm] engine speed
			/// </summary>
			public const string EngineSpeed = "engine speed";

			/// <summary>
			///     [Nm] full load torque
			/// </summary>
			public const string TorqueFullLoad = "full load torque";

			/// <summary>
			///     [Nm] motoring torque
			/// </summary>
			public const string TorqueDrag = "motoring torque";
		}
	}
}
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class RetarderLossMap : SimulationComponentData
	{
		private List<RetarderLossEntry> _entries;


		public static RetarderLossMap ReadFromFile(string fileName)
		{
			var data = VectoCSVFile.Read(fileName);

			if (data.Columns.Count != 2) {
				throw new VectoException("RetarderLossMap Data File must consist of 2 columns.");
			}

			if (data.Rows.Count < 2) {
				throw new VectoException("RetarderLossMap must consist of at leas two entries.");
			}

			List<RetarderLossEntry> entries;
			if (HeaderIsValid(data.Columns)) {
				entries = CreateFromColumnNames(data);
			} else {
				Logger<RetarderLossMap>().Warn(
					"RetarderLossMap: Header Line is not valid. Expected: '{0}, {1}', Got: '{2}'. Falling back to column index.",
					Fields.RetarderSpeed, Fields.TorqueLoss,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));
				entries = CreateFromColumnIndizes(data);
			}
			return new RetarderLossMap { _entries = entries };
		}

		public NewtonMeter RetarderLoss(PerSecond angularVelocity)
		{
			var idx = FindIndex(angularVelocity);
			return VectoMath.Interpolate(_entries[idx - 1].RetarderSpeed, _entries[idx].RetarderSpeed,
				_entries[idx - 1].TorqueLoss, _entries[idx].TorqueLoss, angularVelocity);
		}

		protected int FindIndex(PerSecond angularVelocity)
		{
			int idx;
			if (angularVelocity < _entries[0].RetarderSpeed) {
				Log.Info("requested rpm below minimum rpm in retarder loss map - extrapolating. n: {0}, rpm_min: {1}",
					angularVelocity.ConvertTo().Rounds.Per.Minute, _entries[0].RetarderSpeed.ConvertTo().Rounds.Per.Minute);
				idx = 1;
			} else {
				idx = _entries.FindIndex(x => x.RetarderSpeed > angularVelocity);
			}
			if (idx <= 0) {
				idx = angularVelocity > _entries[0].RetarderSpeed ? _entries.Count - 1 : 1;
			}
			return idx;
		}

		private static List<RetarderLossEntry> CreateFromColumnNames(DataTable data)
		{
			return (from DataRow row in data.Rows
				select new RetarderLossEntry {
					RetarderSpeed = row.ParseDouble(Fields.RetarderSpeed).RPMtoRad(),
					TorqueLoss = row.ParseDouble(Fields.TorqueLoss).SI<NewtonMeter>()
				}).ToList();
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			return columns.Contains(Fields.RetarderSpeed) && columns.Contains(Fields.TorqueLoss);
		}

		private static List<RetarderLossEntry> CreateFromColumnIndizes(DataTable data)
		{
			return (from DataRow row in data.Rows
				select
					new RetarderLossEntry {
						RetarderSpeed = row.ParseDouble(0).RPMtoRad(),
						TorqueLoss = row.ParseDouble(1).SI<NewtonMeter>()
					}).ToList();
		}

		private class RetarderLossEntry
		{
			public PerSecond RetarderSpeed { get; set; }

			public NewtonMeter TorqueLoss { get; set; }
		}

		private static class Fields
		{
			/// <summary>
			///		[rpm]
			/// </summary>
			public const string RetarderSpeed = "Retarder Speed";

			/// <summary>
			///		[Nm]
			/// </summary>
			public const string TorqueLoss = "Torque Loss";
		}
	}
}
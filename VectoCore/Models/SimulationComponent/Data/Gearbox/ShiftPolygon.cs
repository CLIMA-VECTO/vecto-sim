using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Common.Logging;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox
{
	public class ShiftPolygon : SimulationComponentData
	{
		private List<ShiftPolygonEntry> _entries;

		public static ShiftPolygon ReadFromFile(string fileName)
		{
			var data = VectoCSVFile.Read(fileName);

			if (data.Columns.Count != 3) {
				throw new VectoException("ShiftPolygon Data File must contain 3 columns.");
			}

			if (data.Rows.Count < 2) {
				throw new VectoException("ShiftPolygon must consist of at least tow lines with numeric values (below file header)");
			}

			List<ShiftPolygonEntry> entries;
			if (HeaderIsValid(data.Columns)) {
				entries = CreateFromColumnNames(data);
			} else {
				var log = LogManager.GetLogger<ShiftPolygon>();
				log.WarnFormat(
					"ShiftPolygon: Header Line is not valid. Expected: '{0}, {1}, {2}', Got: '{3}'. Falling back to column index",
					Fields.Torque, Fields.AngularSpeedUp, Fields.AngluarSpeedDown,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));
				entries = CreateFromColumnIndizes(data);
			}
			return new ShiftPolygon { _entries = entries };
		}

		public ShiftPolygonEntry this[int i]
		{
			get { return _entries[i]; }
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			return columns.Contains(Fields.Torque) && columns.Contains(Fields.AngularSpeedUp) &&
					columns.Contains((Fields.AngluarSpeedDown));
		}

		private static List<ShiftPolygonEntry> CreateFromColumnNames(DataTable data)
		{
			return (from DataRow row in data.Rows
				select new ShiftPolygonEntry {
					Torque = row.ParseDouble(Fields.Torque).SI<NewtonMeter>(),
					AngularSpeedDown = row.ParseDouble(Fields.AngluarSpeedDown).RPMtoRad(),
					AngularSpeedUp = row.ParseDouble(Fields.AngularSpeedUp).RPMtoRad()
				}).ToList();
		}

		private static List<ShiftPolygonEntry> CreateFromColumnIndizes(DataTable data)
		{
			return (from DataRow row in data.Rows
				select
					new ShiftPolygonEntry {
						Torque = row.ParseDouble(0).SI<NewtonMeter>(),
						AngularSpeedDown = row.ParseDouble(1).RPMtoRad(),
						AngularSpeedUp = row.ParseDouble(2).RPMtoRad()
					}).ToList();
		}

		private static class Fields
		{
			/// <summary>
			///		[Nm] torque
			/// </summary>
			public const string Torque = "engine torque";

			/// <summary>
			///		[rpm] threshold for upshift
			/// </summary>
			public const string AngularSpeedUp = "upshift rpm";

			/// <summary>
			///		[rpm] threshold for downshift
			/// </summary>
			public const string AngluarSpeedDown = "downshift rpm";
		}

		public class ShiftPolygonEntry
		{
			/// <summary>
			///		[Nm] engine torque
			/// </summary>
			public NewtonMeter Torque { get; set; }

			/// <summary>
			///		[1/s] angular velocity threshold for downshift 
			/// </summary>
			public PerSecond AngularSpeedDown { get; set; }

			/// <summary>
			///		[1/s] angular velocity threshold for upshift
			/// </summary>
			public PerSecond AngularSpeedUp { get; set; }
		}
	}
}
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class AccelerationCurveData : SimulationComponentData
	{
		private List<KeyValuePair<MeterPerSecond, AccelerationEntry>> _entries;

		public static AccelerationCurveData ReadFromStream(Stream stream)
		{
			var data = VectoCSVFile.ReadStream(stream);
			return ParseData(data);
		}

		public static AccelerationCurveData ReadFromFile(string fileName)
		{
			var data = VectoCSVFile.Read(fileName);
			return ParseData(data);
		}

		private static AccelerationCurveData ParseData(DataTable data)
		{
			if (data.Columns.Count != 3) {
				throw new VectoException("Acceleration Limiting File must consist of 3 columns.");
			}

			if (data.Rows.Count < 2) {
				throw new VectoException("Acceleration Limiting File must consist of at least two entries.");
			}

			return new AccelerationCurveData {
				_entries = data.Rows.Cast<DataRow>()
					.Select(r => new KeyValuePair<MeterPerSecond, AccelerationEntry>(
						r.ParseDouble("v").SI().Kilo.Meter.Per.Hour.Cast<MeterPerSecond>(),
						new AccelerationEntry {
							Acceleration = r.ParseDouble("acc").SI<MeterPerSquareSecond>(),
							Deceleration = r.ParseDouble("dec").SI<MeterPerSquareSecond>()
						}))
					.OrderBy(x => x.Key)
					.ToList()
			};
		}

		public AccelerationEntry Lookup(MeterPerSecond key)
		{
			var index = 1;
			if (key < _entries[0].Key) {
				Log.ErrorFormat("requested velocity below minimum - extrapolating. velocity: {0}, min: {1}",
					key.ConvertTo().Kilo.Meter.Per.Hour, _entries[0].Key.ConvertTo().Kilo.Meter.Per.Hour);
			} else {
				index = _entries.FindIndex(x => x.Key > key);
				if (index <= 0) {
					index = (key > _entries[0].Key) ? _entries.Count - 1 : 1;
				}
			}
			
			return new AccelerationEntry {
				Acceleration =
					VectoMath.Interpolate(_entries[index - 1].Key, _entries[index].Key, _entries[index - 1].Value.Acceleration,
						_entries[index].Value.Acceleration, key),
				Deceleration =
					VectoMath.Interpolate(_entries[index - 1].Key, _entries[index].Key, _entries[index - 1].Value.Deceleration,
						_entries[index].Value.Deceleration, key)
			};
		}

		public class AccelerationEntry
		{
			public MeterPerSquareSecond Acceleration { get; set; }
			public MeterPerSquareSecond Deceleration { get; set; }
		}
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Engine
{
	[JsonObject(MemberSerialization.Fields)]
	public class FuelConsumptionMap : SimulationComponentData
	{
		private readonly IList<FuelConsumptionEntry> _entries = new List<FuelConsumptionEntry>();
		private readonly DelauneyMap _fuelMap = new DelauneyMap();
		private FuelConsumptionMap() {}

		public static FuelConsumptionMap ReadFromFile(string fileName)
		{
			var fuelConsumptionMap = new FuelConsumptionMap();
			var data = VectoCSVFile.Read(fileName);

			try {
				foreach (DataRow row in data.Rows) {
					try {
						var entry = new FuelConsumptionEntry {
							EngineSpeed =
								row.ParseDouble(Fields.EngineSpeed).SI().Rounds.Per.Minute.Cast<PerSecond>(),
							Torque = row.ParseDouble(Fields.Torque).SI<NewtonMeter>(),
							FuelConsumption =
								row.ParseDouble(Fields.FuelConsumption).SI().Gramm.Per.Hour.ConvertTo().Kilo.Gramm.Per.Second
						};

						if (entry.FuelConsumption < 0) {
							throw new ArgumentOutOfRangeException("FuelConsumption", "FuelConsumption < 0 not allowed.");
						}

						fuelConsumptionMap._entries.Add(entry);

						// Delauney map works only as expected, when the engineSpeed is in rpm.
						fuelConsumptionMap._fuelMap.AddPoint(entry.Torque.Value(), row.ParseDouble(Fields.EngineSpeed),
							entry.FuelConsumption.Value());
					} catch (Exception e) {
						throw new VectoException(string.Format("Line {0}: {1}", data.Rows.IndexOf(row), e.Message), e);
					}
				}
			} catch (Exception e) {
				throw new VectoException(string.Format("File {0}: {1}", fileName, e.Message), e);
			}

			fuelConsumptionMap._fuelMap.Triangulate();
			return fuelConsumptionMap;
		}

		/// <summary>
		///     [kg/s] Calculates the fuel consumption based on the given fuel map,
		///     the engineSpeed [rad/s] and the torque [Nm].
		/// </summary>
		/// <param name="engineSpeed">[rad/sec]</param>
		/// <param name="torque">[Nm]</param>
		/// <returns>[kg/s]</returns>
		public KilogramPerSecond GetFuelConsumption(NewtonMeter torque, PerSecond engineSpeed)
		{
			// delauney map needs is initialised with rpm, therefore the engineSpeed has to be converted.
			return
				_fuelMap.Interpolate(torque.Value(), engineSpeed.ConvertTo().Rounds.Per.Minute.Value())
					.SI()
					.Kilo.Gramm.Per.Second.Cast<KilogramPerSecond>();
		}

		private static class Fields
		{
			/// <summary>
			///     [rpm]
			/// </summary>
			public const string EngineSpeed = "engine speed";

			/// <summary>
			///     [Nm]
			/// </summary>
			public const string Torque = "torque";

			/// <summary>
			///     [g/h]
			/// </summary>
			public const string FuelConsumption = "fuel consumption";
		};

		private class FuelConsumptionEntry
		{
			/// <summary>
			///     engine speed [rad/s]
			/// </summary>
			public PerSecond EngineSpeed { get; set; }

			/// <summary>
			///     Torque [Nm]
			/// </summary>
			public NewtonMeter Torque { get; set; }

			/// <summary>
			///     Fuel consumption [kg/s]
			/// </summary>
			public SI FuelConsumption { get; set; }

			#region Equality members

			private bool Equals(FuelConsumptionEntry other)
			{
				Contract.Requires(other != null);
				return EngineSpeed.Equals(other.EngineSpeed) && Torque.Equals(other.Torque) &&
						FuelConsumption.Equals(other.FuelConsumption);
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
				return Equals((FuelConsumptionEntry)obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					var hashCode = EngineSpeed.GetHashCode();
					hashCode = (hashCode * 397) ^ Torque.GetHashCode();
					hashCode = (hashCode * 397) ^ FuelConsumption.GetHashCode();
					return hashCode;
				}
			}

			#endregion
		}

		#region Equality members

		protected bool Equals(FuelConsumptionMap other)
		{
			return _entries.SequenceEqual(other._entries) && Equals(_fuelMap, other._fuelMap);
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
			return Equals((FuelConsumptionMap)obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				return ((_entries != null ? _entries.GetHashCode() : 0) * 397) ^
						(_fuelMap != null ? _fuelMap.GetHashCode() : 0);
			}
		}

		#endregion
	}
}
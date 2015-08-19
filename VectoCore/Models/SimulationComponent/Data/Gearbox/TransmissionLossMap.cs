using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data.Gearbox
{
	public class TransmissionLossMap
	{
		[JsonProperty] private readonly List<GearLossMapEntry> _entries;

		private readonly double _ratio;

		/// <summary>
		/// [X=Input EngineSpeed, Y=Output Torque] => Z=Input Torque
		/// </summary>
		private readonly DelauneyMap _lossMap;

		private readonly NewtonMeter _minTorque = double.PositiveInfinity.SI<NewtonMeter>();
		private readonly NewtonMeter _maxTorque = double.NegativeInfinity.SI<NewtonMeter>();
		private readonly PerSecond _maxSpeed = double.NegativeInfinity.SI<PerSecond>();
		private readonly PerSecond _minSpeed = double.PositiveInfinity.SI<PerSecond>();

		public static TransmissionLossMap ReadFromFile(string fileName, double gearRatio)
		{
			var data = VectoCSVFile.Read(fileName, true);

			if (data.Columns.Count < 3) {
				throw new VectoException("TransmissionLossMap Data File must consist of at least 3 columns.");
			}

			if (data.Rows.Count < 4) {
				throw new VectoException(
					"TransmissionLossMap must consist of at least four lines with numeric values (below file header");
			}

			List<GearLossMapEntry> entries;
			if (HeaderIsValid(data.Columns)) {
				entries = CreateFromColumnNames(data);
			} else {
				var log = LogManager.GetLogger<TransmissionLossMap>();
				log.WarnFormat(
					"TransmissionLossMap: Header line is not valid. Expected: '{0}, {1}, {2}, <{3}>'. Got: '{4}'. Falling back to column index.",
					Fields.InputSpeed, Fields.InputTorque, Fields.TorqeLoss, Fields.Efficiency,
					string.Join(", ", data.Columns.Cast<DataColumn>().Select(c => c.ColumnName).Reverse()));

				entries = CreateFromColumIndizes(data);
			}

			return new TransmissionLossMap(entries, gearRatio);
		}

		private static bool HeaderIsValid(DataColumnCollection columns)
		{
			return columns.Contains(Fields.InputSpeed) && columns.Contains(Fields.InputTorque) &&
					columns.Contains(Fields.TorqeLoss);
		}

		private static List<GearLossMapEntry> CreateFromColumnNames(DataTable data)
		{
			var hasEfficiency = data.Columns.Contains(Fields.Efficiency);
			return (from DataRow row in data.Rows
				select new GearLossMapEntry {
					InputSpeed = row.ParseDouble(Fields.InputSpeed).RPMtoRad(),
					InputTorque = row.ParseDouble(Fields.InputTorque).SI<NewtonMeter>(),
					TorqueLoss = row.ParseDouble(Fields.TorqeLoss).SI<NewtonMeter>(),
					Efficiency =
						(!hasEfficiency || row[Fields.Efficiency] == DBNull.Value || row[Fields.Efficiency] != null)
							? double.NaN
							: row.ParseDouble(Fields.Efficiency)
				}).ToList();
		}

		private static List<GearLossMapEntry> CreateFromColumIndizes(DataTable data)
		{
			var hasEfficiency = (data.Columns.Count >= 4);
			return (from DataRow row in data.Rows
				select new GearLossMapEntry {
					InputSpeed = row.ParseDouble(0).RPMtoRad(),
					InputTorque = row.ParseDouble(1).SI<NewtonMeter>(),
					TorqueLoss = row.ParseDouble(2).SI<NewtonMeter>(),
					Efficiency = (!hasEfficiency || row[3] == DBNull.Value || row[3] != null) ? double.NaN : row.ParseDouble(3)
				}).ToList();
		}

		private TransmissionLossMap(List<GearLossMapEntry> entries, double gearRatio)
		{
			_ratio = gearRatio;
			_entries = entries;
			_lossMap = new DelauneyMap();
			foreach (var entry in _entries) {
				var outTorque = (entry.InputTorque - entry.TorqueLoss) * _ratio;
				_minTorque = VectoMath.Min(_minTorque, outTorque);
				_maxTorque = VectoMath.Max(_maxTorque, outTorque);

				_minSpeed = VectoMath.Min(_minSpeed, entry.InputSpeed);
				_maxSpeed = VectoMath.Max(_maxSpeed, entry.InputSpeed);

				_lossMap.AddPoint(entry.InputSpeed.Value(), outTorque.Value(), entry.InputTorque.Value());
			}

			_lossMap.Triangulate();
		}

		/// <summary>
		///	Computes the INPUT torque given by the input engineSpeed and the output torque.
		/// </summary>
		/// <param name="inAngularVelocity">Angular speed at input side.</param>
		/// <param name="outTorque">Torque at output side (as requested by the previous componend towards the wheels).</param>
		/// <returns>Torque needed at input side (towards the engine).</returns>
		public NewtonMeter GearboxInTorque(PerSecond inAngularVelocity, NewtonMeter outTorque)
		{
			try {
				var limitedAngularVelocity = VectoMath.Limit(inAngularVelocity, _minSpeed, _maxSpeed).Value();
				var limitedTorque = VectoMath.Limit(outTorque, _minTorque, _maxTorque).Value();

				var inTorque = _lossMap.Interpolate(limitedAngularVelocity, limitedTorque).SI<NewtonMeter>();
				LogManager.GetLogger(GetType()).DebugFormat("GearboxLoss: {0}", inTorque - outTorque);

				// Limit input torque to a maximum value without losses (just torque/ratio)
				return VectoMath.Max(inTorque, outTorque / _ratio);
			} catch (VectoException) {
				LogManager.GetLogger(GetType())
					.ErrorFormat("Failed to interpolate in TransmissionLossMap. angularVelocity: {0}, torque: {1}", inAngularVelocity,
						outTorque);
				return outTorque / _ratio;
			}
		}

		public GearLossMapEntry this[int i]
		{
			get { return _entries[i]; }
		}

		public class GearLossMapEntry
		{
			public PerSecond InputSpeed { get; set; }

			public NewtonMeter InputTorque { get; set; }

			public NewtonMeter TorqueLoss { get; set; }

			public double Efficiency { get; set; }
		}

		private static class Fields
		{
			/// <summary>[rpm]</summary>
			public const string InputSpeed = "Input Speed";

			/// <summary>[Nm]</summary>
			public const string InputTorque = "Input Torque";

			/// <summary>[Nm]</summary>
			public const string TorqeLoss = "Torque Loss";

			/// <summary>[-]</summary>
			public const string Efficiency = "Eff";
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.SimulationComponent.Data.Engine;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	/// <summary>
	///     Represents the CombustionEngineData. Fileformat: .veng
	/// </summary>
	/// <code>
	/// {
	///  "Header": {
	///    "CreatedBy": " ()",
	///    "Date": "3/4/2015 12:26:24 PM",
	///    "AppVersion": "2.0.4-beta3",
	///    "FileVersion": 2
	///  },
	///  "Body": {
	///    "SavedInDeclMode": false,
	///    "ModelName": "Generic 24t Coach",
	///    "Displacement": 12730.0,
	///    "IdlingSpeed": 560.0,
	///    "Inertia": 3.8,
	///    "FullLoadCurves": [
	///      {
	///        "Path": "24t Coach.vfld",
	///        "Gears": "0 - 99"
	///      }
	///    ],
	///    "FuelMap": "24t Coach.vmap",
	///    "WHTC-Urban": 0.0,
	///    "WHTC-Rural": 0.0,
	///    "WHTC-Motorway": 0.0
	///  }
	/// }
	/// </code>
	[DataContract]
	public class CombustionEngineData : SimulationComponentData
	{
		[DataMember] private readonly Dictionary<Range, FullLoadCurve> _fullLoadCurves =
			new Dictionary<Range, FullLoadCurve>();

		[DataMember] private Data _data;

		public bool SavedInDeclarationMode
		{
			get { return _data.Body.SavedInDeclarationMode; }
			protected set { _data.Body.SavedInDeclarationMode = value; }
		}

		public string ModelName
		{
			get { return _data.Body.ModelName; }
			protected set { _data.Body.ModelName = value; }
		}

		/// <summary>
		///     [m^3]
		/// </summary>
		public SI Displacement
		{
			get { return _data.Body.Displacement.SI().Cubic.Centi.Meter.ConvertTo().Cubic.Meter.Value(); }
			protected set { _data.Body.Displacement = (double)value.ConvertTo().Cubic.Centi.Meter; }
		}

		/// <summary>
		///     [rad/s]
		/// </summary>
		public PerSecond IdleSpeed
		{
			get { return _data.Body.IdleSpeed.RPMtoRad(); }
			protected set { _data.Body.IdleSpeed = (double)value.ConvertTo().Rounds.Per.Minute; }
		}

		/// <summary>
		///     [kgm^2]
		/// </summary>
		public KilogramSquareMeter Inertia
		{
			get { return _data.Body.Inertia.SI<KilogramSquareMeter>(); }
			protected set { _data.Body.Inertia = (double)value.ConvertTo().Kilo.Gramm.Square.Meter; }
		}

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public SI WHTCUrban
		{
			get { return _data.Body.WHTCUrban.SI().Gramm.Per.Kilo.Watt.Hour.ConvertTo().Kilo.Gramm.Per.Watt.Second.Value(); }
			protected set { _data.Body.WHTCUrban = (double)value.ConvertTo().Gramm.Per.Kilo.Watt.Hour; }
		}

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public SI WHTCRural
		{
			get { return _data.Body.WHTCRural.SI().Gramm.Per.Kilo.Watt.Hour.ConvertTo().Kilo.Gramm.Per.Watt.Second.Value(); }
			protected set { _data.Body.WHTCRural = (double)value.ConvertTo().Gramm.Per.Kilo.Watt.Hour; }
		}

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public SI WHTCMotorway
		{
			get { return _data.Body.WHTCMotorway.SI().Gramm.Per.Kilo.Watt.Hour.ConvertTo().Kilo.Gramm.Per.Watt.Second.Value(); }
			protected set { _data.Body.WHTCMotorway = (double)value.ConvertTo().Gramm.Per.Kilo.Watt.Hour; }
		}

		[DataMember]
		public FuelConsumptionMap ConsumptionMap { get; set; }

		public static CombustionEngineData ReadFromFile(string fileName)
		{
			return ReadFromJson(File.ReadAllText(fileName), Path.GetDirectoryName(fileName));
		}

		public static CombustionEngineData ReadFromJson(string json, string basePath = "")
		{
			var combustionEngineData = new CombustionEngineData();
			//todo handle conversion errors
			var d = JsonConvert.DeserializeObject<Data>(json);

			combustionEngineData._data = d;

			if (d.Header.FileVersion > 2) {
				throw new UnsupportedFileVersionException("Unsupported Version of .veng file. Got Version: " +
														d.Header.FileVersion);
			}

			combustionEngineData.ConsumptionMap = FuelConsumptionMap.ReadFromFile(Path.Combine(basePath, d.Body.FuelMap));

			foreach (var loadCurve in d.Body.FullLoadCurves) {
				var fullLoadCurve = FullLoadCurve.ReadFromFile(Path.Combine(basePath, loadCurve.Path));
				var gearRange = new Range(loadCurve.Gears);
				combustionEngineData._fullLoadCurves[gearRange] = fullLoadCurve;
			}

			return combustionEngineData;
		}

		public void WriteToFile(string fileName)
		{
			//todo handle file exceptions
			File.WriteAllText(fileName, ToJson());
		}

		public string ToJson()
		{
			_data.Header.Date = DateTime.Now;
			_data.Header.FileVersion = 2;
			_data.Header.AppVersion = "3.0.0"; // todo: get current app version!
			_data.Header.CreatedBy = ""; // todo: get current user
			_data.Body.SavedInDeclarationMode = false; //todo: get declaration mode setting
			return JsonConvert.SerializeObject(_data, Formatting.Indented);
		}

		public FullLoadCurve GetFullLoadCurve(uint gear)
		{
			var curve = _fullLoadCurves.FirstOrDefault(kv => kv.Key.Contains(gear));
			if (curve.Key == null) {
				throw new KeyNotFoundException(string.Format("GearData '{0}' was not found in the FullLoadCurves.", gear));
			}

			return curve.Value;
		}

		/// <summary>
		///     A class which represents the json data format for serializing and deserializing the EngineData files.
		/// </summary>
		public class Data
		{
			[JsonProperty(Required = Required.Always)] public JsonDataHeader Header;
			[JsonProperty(Required = Required.Always)] public DataBody Body;

			public class DataBody
			{
				/// <summary>
				///     [ccm] Displacement in cubic centimeter.
				///     Used in Declaration Mode to calculate inertia.
				/// </summary>
				[JsonProperty(Required = Required.Always)] public double Displacement;

				/// <summary>
				///     The Fuel Consumption Map is used to calculate the base Fuel Consumption (FC) value.
				/// </summary>
				[JsonProperty(Required = Required.Always)] public string FuelMap;

				[JsonProperty(Required = Required.Always)] public IList<DataFullLoadCurve> FullLoadCurves;

				/// <summary>
				///     [rpm] Idling Engine Speed
				///     Low idle, applied in simulation for vehicle standstill in neutral gear position.
				/// </summary>
				[JsonProperty("IdlingSpeed", Required = Required.Always)] public double IdleSpeed;

				/// <summary>
				///     [kgm^2] Inertia including Flywheel
				///     Inertia for rotating parts including engine flywheel.
				///     In Declaration Mode the inertia is calculated automatically.
				/// </summary>
				[JsonProperty(Required = Required.Always)] public double Inertia;

				/// <summary>
				///     Model. Free text defining the engine model, type, etc.
				/// </summary>
				[JsonProperty(Required = Required.Always)] public string ModelName;

				[JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

				/// <summary>
				///     [g/kWh] The WHTC test results are required in Declaration Mode for the motorway WHTC FC Correction.
				/// </summary>
				[JsonProperty("WHTC-Motorway")] public double WHTCMotorway;

				/// <summary>
				///     [g/kWh] The WHTC test results are required in Declaration Mode for the rural WHTC FC Correction.
				/// </summary>
				[JsonProperty("WHTC-Rural")] public double WHTCRural;

				/// <summary>
				///     [g/kWh] The WHTC test results are required in Declaration Mode for the urban WHTC FC Correction.
				/// </summary>
				[JsonProperty("WHTC-Urban")] public double WHTCUrban;

				/// <summary>
				///     Multiple Full Load and Drag Curves (.vfld) can be defined and assigned to different gears.
				///     GearData "0" must be assigned for idling and Engine Only Mode.
				/// </summary>
				public class DataFullLoadCurve
				{
					[JsonProperty(Required = Required.Always)] public string Gears;
					[JsonProperty(Required = Required.Always)] public string Path;

					#region Equality Members

					protected bool Equals(DataFullLoadCurve other)
					{
						return string.Equals(Path, other.Path) && string.Equals(Gears, other.Gears);
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
						return Equals((DataFullLoadCurve)obj);
					}

					public override int GetHashCode()
					{
						unchecked {
							return ((Path != null ? Path.GetHashCode() : 0) * 397) ^
									(Gears != null ? Gears.GetHashCode() : 0);
						}
					}

					#endregion
				}

				#region Equality members

				protected bool Equals(DataBody other)
				{
					return SavedInDeclarationMode.Equals(other.SavedInDeclarationMode)
							&& string.Equals(ModelName, other.ModelName)
							&& Displacement.Equals(other.Displacement)
							&& IdleSpeed.Equals(other.IdleSpeed)
							&& Inertia.Equals(other.Inertia)
							&& FullLoadCurves.SequenceEqual(other.FullLoadCurves)
							&& string.Equals(FuelMap, other.FuelMap)
							&& WHTCUrban.Equals(other.WHTCUrban)
							&& WHTCRural.Equals(other.WHTCRural)
							&& WHTCMotorway.Equals(other.WHTCMotorway);
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
					return Equals((DataBody)obj);
				}

				public override int GetHashCode()
				{
					unchecked {
						var hashCode = SavedInDeclarationMode.GetHashCode();
						hashCode = (hashCode * 397) ^ (ModelName != null ? ModelName.GetHashCode() : 0);
						hashCode = (hashCode * 397) ^ Displacement.GetHashCode();
						hashCode = (hashCode * 397) ^ IdleSpeed.GetHashCode();
						hashCode = (hashCode * 397) ^ Inertia.GetHashCode();
						hashCode = (hashCode * 397) ^ (FullLoadCurves != null ? FullLoadCurves.GetHashCode() : 0);
						hashCode = (hashCode * 397) ^ (FuelMap != null ? FuelMap.GetHashCode() : 0);
						hashCode = (hashCode * 397) ^ WHTCUrban.GetHashCode();
						hashCode = (hashCode * 397) ^ WHTCRural.GetHashCode();
						hashCode = (hashCode * 397) ^ WHTCMotorway.GetHashCode();
						return hashCode;
					}
				}

				#endregion
			}

			#region Equality members

			protected bool Equals(Data other)
			{
				return Equals(Header, other.Header) && Equals(Body, other.Body);
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
				return Equals((Data)obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					return ((Header != null ? Header.GetHashCode() : 0) * 397) ^ (Body != null ? Body.GetHashCode() : 0);
				}
			}

			#endregion
		}

		public class RangeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				return value.GetType() == typeof(string)
					? new Range((string)value)
					: base.ConvertFrom(context, culture, value);
			}
		}

		[TypeConverter(typeof(RangeConverter))]
		private class Range
		{
			private readonly uint _end;
			private readonly uint _start;

			public Range(string range)
			{
				Contract.Requires(range != null);

				_start = uint.Parse(range.Split('-').First().Trim());
				_end = uint.Parse(range.Split('-').Last().Trim());
			}

			public override string ToString()
			{
				return string.Format("{0} - {1}", _start, _end);
			}

			public bool Contains(uint value)
			{
				return _start <= value && value <= _end;
			}

			#region Equality members

			protected bool Equals(Range other)
			{
				Contract.Requires(other != null);
				return _start == other._start && _end == other._end;
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
				return Equals((Range)obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					return (int)((_start * 397) ^ _end);
				}
			}

			#endregion
		}

		#region Equality members

		protected bool Equals(CombustionEngineData other)
		{
			return Equals(_data, other._data)
					&& _fullLoadCurves.Keys.SequenceEqual(other._fullLoadCurves.Keys)
					&& _fullLoadCurves.Values.SequenceEqual(other._fullLoadCurves.Values)
					&& Equals(ConsumptionMap, other.ConsumptionMap);
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
			return Equals((CombustionEngineData)obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = _data.GetHashCode();
				hashCode = (hashCode * 397) ^ (_fullLoadCurves != null ? _fullLoadCurves.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ConsumptionMap != null ? ConsumptionMap.GetHashCode() : 0);
				return hashCode;
			}
		}

		#endregion
	}
}
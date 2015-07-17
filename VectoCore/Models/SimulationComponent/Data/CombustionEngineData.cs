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
	public class CombustionEngineData : SimulationComponentData
	{
		private readonly Dictionary<Range, FullLoadCurve> _fullLoadCurves =
			new Dictionary<Range, FullLoadCurve>();


		public string ModelName { get; internal set; }


		/// <summary>
		///     [m^3]
		/// </summary>
		public CubicMeter Displacement { get; internal set; }

		/// <summary>
		///     [rad/s]
		/// </summary>
		public PerSecond IdleSpeed { get; internal set; }


		/// <summary>
		///     [kgm^2]
		/// </summary>
		public KilogramSquareMeter Inertia { get; internal set; }

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public KilogramPerWattSecond WHTCUrban { get; internal set; }

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public KilogramPerWattSecond WHTCRural { get; internal set; }

		/// <summary>
		///     [kg/Ws]
		/// </summary>
		public KilogramPerWattSecond WHTCMotorway { get; internal set; }

		public FuelConsumptionMap ConsumptionMap { get; internal set; }


		public FullLoadCurve GetFullLoadCurve(uint gear)
		{
			var curve = _fullLoadCurves.FirstOrDefault(kv => kv.Key.Contains(gear));
			if (curve.Key == null) {
				throw new KeyNotFoundException(string.Format("GearData '{0}' was not found in the FullLoadCurves.", gear));
			}

			return curve.Value;
		}

		internal void AddFullLoadCurve(string gears, FullLoadCurve fullLoadCurve)
		{
			var range = new Range(gears);
			if (!_fullLoadCurves.ContainsKey(range)) {
				fullLoadCurve.EngineData = this;
				_fullLoadCurves.Add(range, fullLoadCurve);
			} else {
				throw new VectoException(String.Format("FullLoadCurve for gears {0} already specified!", gears));
			}
		}

		protected bool Equals(CombustionEngineData other)
		{
			return Equals(_fullLoadCurves, other._fullLoadCurves) && string.Equals(ModelName, other.ModelName) &&
					Equals(Displacement, other.Displacement) && Equals(IdleSpeed, other.IdleSpeed) && Equals(Inertia, other.Inertia) &&
					Equals(WHTCUrban, other.WHTCUrban) && Equals(WHTCRural, other.WHTCRural) &&
					Equals(WHTCMotorway, other.WHTCMotorway) && Equals(ConsumptionMap, other.ConsumptionMap);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (ReferenceEquals(this, obj)) {
				return true;
			}
			if (obj.GetType() != this.GetType()) {
				return false;
			}
			return Equals((CombustionEngineData) obj);
		}

		public override int GetHashCode()
		{
			unchecked {
				var hashCode = (_fullLoadCurves != null ? _fullLoadCurves.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ModelName != null ? ModelName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Displacement != null ? Displacement.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (IdleSpeed != null ? IdleSpeed.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Inertia != null ? Inertia.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (WHTCUrban != null ? WHTCUrban.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (WHTCRural != null ? WHTCRural.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (WHTCMotorway != null ? WHTCMotorway.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ConsumptionMap != null ? ConsumptionMap.GetHashCode() : 0);
				return hashCode;
			}
		}


		public class RangeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			{
				return sourceType == typeof (string) || base.CanConvertFrom(context, sourceType);
			}

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				return value.GetType() == typeof (string)
					? new Range((string) value)
					: base.ConvertFrom(context, culture, value);
			}
		}

		[TypeConverter(typeof (RangeConverter))]
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
				return Equals((Range) obj);
			}

			public override int GetHashCode()
			{
				unchecked {
					return (int) ((_start * 397) ^ _end);
				}
			}

			#endregion
		}
	}
}
using System;
using System.CodeDom;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	[DesignerCategory("")] // Full qualified attribute needed to disable design view in VisualStudio
	public class ModalResults : DataTable
	{
		public static class ExtendedPropertyNames
		{
			public const string Decimals = "decimals";
			public const string OutputFactor = "outputFactor";
			public const string ShowUnit = "showUnit";
		}


		public ModalResults()
		{
			foreach (var value in EnumHelper.GetValues<ModalResultField>()) {
				var col = new DataColumn(value.GetName(), value.GetAttribute().DataType) { Caption = value.GetCaption() };
				col.ExtendedProperties[ExtendedPropertyNames.Decimals] = value.GetAttribute().Decimals;
				col.ExtendedProperties[ExtendedPropertyNames.OutputFactor] = value.GetAttribute().OutputFactor;
				col.ExtendedProperties[ExtendedPropertyNames.ShowUnit] = value.GetAttribute().ShowUnit;
				Columns.Add(col);
			}
		}

		public static ModalResults ReadFromFile(string fileName)
		{
			var modalResults = new ModalResults();
			var data = VectoCSVFile.Read(fileName);

			foreach (DataRow row in data.Rows) {
				try {
					var newRow = modalResults.NewRow();
					foreach (DataColumn col in row.Table.Columns) {
						// In cols FC-AUXc and FC-WHTCc can be a "-"
						if (row.Field<string>(col) == "-"
							&& (col.ColumnName == ModalResultField.FCAUXc.GetName() || col.ColumnName == ModalResultField.FCWHTCc.GetName())) {
							continue;
						}

						// In col FC can sometimes be a "ERROR"
						if (row.Field<string>(col) == "ERROR" && col.ColumnName == ModalResultField.FCMap.GetName()) {
							continue;
						}

						if (col.ColumnName.StartsWith(ModalResultField.Paux_.ToString()) && !modalResults.Columns.Contains(col.ColumnName)) {
							modalResults.Columns.Add(col.ColumnName, typeof(SI));
						}

						if (typeof(SI).IsAssignableFrom(modalResults.Columns[col.ColumnName].DataType)) {
							newRow.SetField(col.ColumnName, row.ParseDoubleOrGetDefault(col.ColumnName).SI());
						} else {
							newRow.SetField(col.ColumnName, row.ParseDoubleOrGetDefault(col.ColumnName));
						}
					}
					modalResults.Rows.Add(newRow);
				} catch (VectoException ex) {
					throw new VectoException(string.Format("Row {0}: {1}", data.Rows.IndexOf(row), ex.Message), ex);
				}
			}

			return modalResults;
		}

		public void WriteToFile(string fileName)
		{
			VectoCSVFile.Write(fileName, this);
		}
	}

	/// <summary>
	///     Enum with field definitions of the Modal Results File (.vmod).
	/// </summary>
	public enum ModalResultField
	{
		/// <summary>
		///     Time step [s].
		///     Midpoint of the simulated interval.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "time [s]")] time,

		/// <summary>
		///     Simulation interval around the current time step. [s]
		/// </summary>
		[ModalResultField(typeof(SI), "simulation_interval", "dt [s]")] simulationInterval,

		/// <summary>
		///     Engine speed [1/min].
		/// </summary>
		[ModalResultField(typeof(SI), caption: "n [1/min]", outputFactor: 60 / (2 * Math.PI))] n,

		/// <summary>
		///     [Nm]	Engine torque.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Tq_eng [Nm]")] Tq_eng,

		/// <summary>
		///     [Nm]	Torque at clutch (before clutch, engine-side)
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Tq_clutch [Nm]")] Tq_clutch,

		/// <summary>
		///     [Nm]	Full load torque
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Tq_full [Nm]")] Tq_full,

		/// <summary>
		///     [Nm]	Motoring torque
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Tq_drag [Nm]")] Tq_drag,

		/// <summary>
		///     [kW]	Engine power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_eng [kW]", outputFactor: 1e-3)] Pe_eng,

		/// <summary>
		///     [kW]	Engine full load power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_full [kW]", outputFactor: 1e-3)] Pe_full,

		/// <summary>
		///     [kW]	Engine drag power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_drag [kW]", outputFactor: 1e-3)] Pe_drag,

		/// <summary>
		///     [kW]	Engine power at clutch (equals Pe minus loss due to rotational inertia Pa Eng).
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_clutch [kW]", outputFactor: 1e-3)] Pe_clutch,

		/// <summary>
		///     [kW]	Rotational acceleration power: Engine.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Pa Eng", caption: "Pa Eng [kW]", outputFactor: 1e-3)] PaEng,

		/// <summary>
		///     [kW]	Total auxiliary power demand .
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Paux [kW]", outputFactor: 1e-3)] Paux,

		/// <summary>
		///     [g/h]	Fuel consumption from FC map..
		/// </summary>
		[ModalResultField(typeof(SI), name: "FC-Map", caption: "FC-Map [g/h]", outputFactor: 3600 * 1000)] FCMap,

		/// <summary>
		///     [g/h]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
		/// </summary>
		[ModalResultField(typeof(SI), name: "FC-AUXc", caption: "FC-AUXc [g/h]", outputFactor: 3600)] FCAUXc,

		/// <summary>
		///     [g/h]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
		/// </summary>
		[ModalResultField(typeof(SI), name: "FC-WHTCc", caption: "FC-WHTCc [g/h]", outputFactor: 3600)] FCWHTCc,

		/// <summary>
		///     [km]	Travelled distance.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "dist [m]")] dist,

		/// <summary>
		///     [km/h]	Actual vehicle speed.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "v_act [km/h]", outputFactor: 3.6)] v_act,

		/// <summary>
		///     [km/h]	Target vehicle speed.
		/// </summary>
		[ModalResultField(typeof(SI), outputFactor: 3.6)] v_targ,

		/// <summary>
		///     [m/s2]	Vehicle acceleration.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "acc [m/s^2]")] acc,

		/// <summary>
		///     [%]	    Road gradient.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "grad [%]")] grad,

		/// <summary>
		///     [-]	 GearData. "0" = clutch opened / neutral. "0.5" = lock-up clutch is open (AT with torque converter only, see
		///     Gearbox)
		/// </summary>
		[ModalResultField(typeof(uint), caption: "Gear [-]")] Gear,

		/// <summary>
		///     [kW]	Gearbox losses.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Ploss GB", caption: "Ploss GB [kW]", outputFactor: 1e-3)] PlossGB,

		/// <summary>
		///     [kW]	Losses in differential / axle transmission.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Ploss Diff", caption: "Ploss Diff [kW]", outputFactor: 1e-3)] PlossDiff,

		/// <summary>
		///     [kW]	Retarder losses.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Ploss Retarder", caption: "Ploss Retarder [kW]", outputFactor: 1e-3)] PlossRetarder,

		/// <summary>
		///     [kW]	Rotational acceleration power: Gearbox.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Pa GB", caption: "Pa GB [kW]", outputFactor: 1e-3)] PaGB,

		/// <summary>
		///     [kW]	Vehicle acceleration power.
		/// </summary>
		[ModalResultField(typeof(SI), name: "Pa Veh", caption: "Pa Veh [kW]", outputFactor: 1e-3)] PaVeh,

		/// <summary>
		///     [kW]	Rolling resistance power demand.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Proll [kW]", outputFactor: 1e-3)] Proll,

		/// <summary>
		///     [kW]	Air resistance power demand.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pair [kW]", outputFactor: 1e-3)] Pair,

		/// <summary>
		///     [kW]	Power demand due to road gradient.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pgrad [kW]", outputFactor: 1e-3)] Pgrad,

		/// <summary>
		///     [kW]	Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pwheel [kW]", outputFactor: 1e-3)] Pwheel,

		/// <summary>
		///     [kW]	Brake power. Drag power is included in Pe.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pbrake [kW]", outputFactor: 1e-3)] Pbrake,

		/// <summary>
		///     [kW]	Power demand of Auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
		/// </summary>
		[ModalResultField(typeof(SI), outputFactor: 1e-3)] Paux_,

		/// <summary>
		///     [-]	    Torque converter speed ratio
		/// </summary>
		[ModalResultField(typeof(SI), name: "TCν")] TCv,

		/// <summary>
		///     [-]	    Torque converter torque ratio
		/// </summary>
		[ModalResultField(typeof(SI), name: "TCµ")] TCmu,

		/// <summary>
		///     [Nm]	Torque converter output torque
		/// </summary>
		[ModalResultField(typeof(SI))] TC_M_Out,

		/// <summary>
		///     [1/min]	Torque converter output speed
		/// </summary>
		[ModalResultField(typeof(SI))] TC_n_Out,

		/// <summary>
		///     [m]	Altitude
		/// </summary>
		[ModalResultField(typeof(SI))] altitude,
	}


	[AttributeUsage(AttributeTargets.Field)]
	public class ModalResultFieldAttribute : Attribute
	{
		internal ModalResultFieldAttribute(Type dataType, string name = null, string caption = null, uint decimals = 4,
			double outputFactor = 1, bool showUnit = false)
		{
			DataType = dataType;
			Name = name;
			Caption = caption;
			Decimals = decimals;
			OutputFactor = outputFactor;
			ShowUnit = showUnit;
		}

		public bool ShowUnit { get; private set; }
		public double OutputFactor { get; private set; }
		public uint Decimals { get; private set; }
		public Type DataType { get; private set; }
		public string Name { get; private set; }
		public string Caption { get; private set; }
	}

	public static class ModalResultFieldExtensionMethods
	{
		public static string GetName(this ModalResultField field)
		{
			return GetAttribute(field).Name ?? field.ToString();
		}

		public static string GetCaption(this ModalResultField field)
		{
			return GetAttribute(field).Caption ?? GetAttribute(field).Name ?? field.ToString();
		}

		public static ModalResultFieldAttribute GetAttribute(this ModalResultField field)
		{
			return (ModalResultFieldAttribute)Attribute.GetCustomAttribute(ForValue(field), typeof(ModalResultFieldAttribute));
		}

		private static MemberInfo ForValue(ModalResultField field)
		{
			return typeof(ModalResultField).GetField(Enum.GetName(typeof(ModalResultField), field));
		}
	}
}
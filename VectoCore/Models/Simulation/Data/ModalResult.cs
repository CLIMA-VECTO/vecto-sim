using System;
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
		public ModalResults()
		{
			foreach (var value in EnumHelper.GetValues<ModalResultField>()) {
				var col = new DataColumn(value.GetName(), value.GetDataType()) { Caption = value.GetCaption() };
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
							modalResults.Columns.Add(col.ColumnName, typeof(double));
						}

						newRow.SetField(col.ColumnName, row.ParseDoubleOrGetDefault(col.ColumnName));
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
		[ModalResultField(typeof(SI), caption: "n [1/min]")] n,

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
		[ModalResultField(typeof(SI), caption: "Pe_eng [kW]")] Pe_eng,

		/// <summary>
		///     [kW]	Engine full load power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_full [kW]")] Pe_full,

		/// <summary>
		///     [kW]	Engine drag power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_drag [kW]")] Pe_drag,

		/// <summary>
		///     [kW]	Engine power at clutch (equals Pe minus loss due to rotational inertia Pa Eng).
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pe_clutch [kW]")] Pe_clutch,

		/// <summary>
		///     [kW]	Rotational acceleration power: Engine.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pa Eng [kW]")] PaEng,

		/// <summary>
		///     [kW]	Total auxiliary power demand .
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Paux [kW]")] Paux,

		/// <summary>
		///     [g/h]	Fuel consumption from FC map..
		/// </summary>
		[ModalResultField(typeof(SI), caption: "FC-Map [g/h]")] FCMap,

		/// <summary>
		///     [g/h]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
		/// </summary>
		[ModalResultField(typeof(SI), caption: "FC-AUXc [g/h]")] FCAUXc,

		/// <summary>
		///     [g/h]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
		/// </summary>
		[ModalResultField(typeof(SI), caption: "FC-WHTCc [g/h]")] FCWHTCc,

		/// <summary>
		///     [km]	Travelled distance.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "dist [m]")] dist,

		/// <summary>
		///     [km/h]	Actual vehicle speed.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "v_act [km/h]")] v_act,

		/// <summary>
		///     [km/h]	Target vehicle speed.
		/// </summary>
		[ModalResultField(typeof(SI))] v_targ,

		/// <summary>
		///     [m/s2]	Vehicle acceleration.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "acc [m/s^2]")] acc,

		/// <summary>
		///     [%]	    Road gradient.
		/// </summary>
		[ModalResultField(typeof(double), caption: "grad [%]")] grad,

		/// <summary>
		///     [-]	 GearData. "0" = clutch opened / neutral. "0.5" = lock-up clutch is open (AT with torque converter only, see
		///     Gearbox)
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Gear [-]")] Gear,

		/// <summary>
		///     [kW]	Gearbox losses.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Ploss GB [kW]")] PlossGB,

		/// <summary>
		///     [kW]	Losses in differential / axle transmission.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Ploss Diff [kW]")] PlossDiff,

		/// <summary>
		///     [kW]	Retarder losses.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pa GB [kW]")] PlossRetarder,

		/// <summary>
		///     [kW]	Rotational acceleration power: Gearbox.
		/// </summary>
		[ModalResultField(typeof(SI), "Pa GB")] PaGB,

		/// <summary>
		///     [kW]	Vehicle acceleration power.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pa Veh [kW]")] PaVeh,

		/// <summary>
		///     [kW]	Rolling resistance power demand.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Proll [kW]")] Proll,

		/// <summary>
		///     [kW]	Air resistance power demand.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pair [kW]")] Pair,

		/// <summary>
		///     [kW]	Power demand due to road gradient.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pgrad [kW]")] Pgrad,

		/// <summary>
		///     [kW]	Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pwheel [kW]")] Pwheel,

		/// <summary>
		///     [kW]	Brake power. Drag power is included in Pe.
		/// </summary>
		[ModalResultField(typeof(SI), caption: "Pbrake [kW]")] Pbrake,

		/// <summary>
		///     [kW]	Power demand of Auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
		/// </summary>
		[ModalResultField(typeof(SI))] Paux_,

		/// <summary>
		///     [-]	    Torque converter speed ratio
		/// </summary>
		[ModalResultField(typeof(SI))] TCv,

		/// <summary>
		///     [-]	    Torque converter torque ratio
		/// </summary>
		[ModalResultField(typeof(SI), caption: "TCµ")] TCmu,

		/// <summary>
		///     [Nm]	Torque converter output torque
		/// </summary>
		[ModalResultField(typeof(SI))] TC_M_Out,

		/// <summary>
		///     [1/min]	Torque converter output speed
		/// </summary>
		[ModalResultField(typeof(SI))] TC_n_Out
	}


	[AttributeUsage(AttributeTargets.Field)]
	internal class ModalResultFieldAttribute : Attribute
	{
		internal ModalResultFieldAttribute(Type fieldType, string name = null, string caption = null)
		{
			FieldType = fieldType;
			Name = name;
			Caption = caption;
		}

		public Type FieldType { get; private set; }
		public string Name { get; private set; }
		public string Caption { get; set; }
	}

	public static class ModalResultFieldExtensionMethods
	{
		public static Type GetDataType(this ModalResultField field)
		{
			return GetAttr(field).FieldType;
		}

		public static string GetName(this ModalResultField field)
		{
			return GetAttr(field).Name ?? field.ToString();
		}

		public static string GetCaption(this ModalResultField field)
		{
			return GetAttr(field).Caption ?? field.GetName();
		}

		private static ModalResultFieldAttribute GetAttr(ModalResultField field)
		{
			return (ModalResultFieldAttribute)Attribute.GetCustomAttribute(ForValue(field), typeof(ModalResultFieldAttribute));
		}

		private static MemberInfo ForValue(ModalResultField field)
		{
			return typeof(ModalResultField).GetField(Enum.GetName(typeof(ModalResultField), field));
		}
	}
}
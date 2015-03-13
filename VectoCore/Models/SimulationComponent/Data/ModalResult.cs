using System;
using System.Data;
using System.Reflection;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    [System.ComponentModel.DesignerCategory("")] // to disable design view in VisualStudio
    public class ModalResults : DataTable
    {
        public ModalResults()
        {
            foreach (ModalResultField value in Enum.GetValues(typeof(ModalResultField)))
                Columns.Add(value.GetName(), value.GetDataType());
        }

        public static ModalResults ReadFromFile(string fileName)
        {
            var modalResults = new ModalResults();
            var data = VectoCSVFile.Read(fileName);

            foreach (DataRow row in data.Rows)
            {
                var newRow = modalResults.NewRow();
                foreach (DataColumn col in row.Table.Columns)
                {
                    // In cols FC-AUXc and FC-WHTCc can be a "-"
                    if ((col.ColumnName == ModalResultField.FCAUXc.GetName() ||
                         col.ColumnName == ModalResultField.FCWHTCc.GetName()) && row.Field<string>(col) == "-")
                        continue;

                    // In col FC can sometimes be a "ERROR"
                    if (col.ColumnName == ModalResultField.FC.GetName() && row.Field<string>(col) == "ERROR")
                        continue;

                    newRow.SetField(col.ColumnName, row.GetDouble(col.ColumnName));

                }
                modalResults.Rows.Add(newRow);
            }

            return modalResults;
        }

        public void WriteToFile(string fileName)
        {
            VectoCSVFile.Write(fileName, this);
        }
    }

    /// <summary>
    /// Enum with field definitions of the Modal Results File (.vmod).
    /// </summary>
    public enum ModalResultField
    {
        [ModalResultField(typeof(double))] time,			//	[s]	    Time step.
		[ModalResultField(typeof(double))] n,			    //	[1/min]	Engine speed.
		[ModalResultField(typeof(double))] Tq_eng,			//	[Nm]	Engine torque.
		[ModalResultField(typeof(double))] Tq_clutch,		//	[Nm]	Torque at clutch (before clutch, engine-side)
		[ModalResultField(typeof(double))] Tq_full,			//	[Nm]	Full load torque
		[ModalResultField(typeof(double))] Tq_drag,			//	[Nm]	Motoring torque
		[ModalResultField(typeof(double))] Pe_eng,			//	[kW]	Engine power.
		[ModalResultField(typeof(double))] Pe_full,			//	[kW]	Engine full load power.
		[ModalResultField(typeof(double))] Pe_drag,			//	[kW]	Engine drag power.
		[ModalResultField(typeof(double))] Pe_clutch,		//	[kW]	Engine power at clutch (equals Pe minus loss due to rotational inertia Pa Eng).
		[ModalResultField(typeof(double), "Pa")] PaEng,			//	[kW]	Rotational acceleration power: Engine.
		[ModalResultField(typeof(double))] Paux,			//	[kW]	Total auxiliary power demand .
		[ModalResultField(typeof(double))] FC,			    //	[g/h]	Fuel consumption from FC map..
		[ModalResultField(typeof(double), "FC-AUXc")] FCAUXc,			//	[g/h]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
		[ModalResultField(typeof(double), "FC-WHTCc")] FCWHTCc,		//	[g/h]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)


        [ModalResultField(typeof(double))] dist,			//	[km]	Travelled distance.
        [ModalResultField(typeof(double))] v_act,			//	[km/h]	Actual vehicle speed.
		[ModalResultField(typeof(double))] v_targ,			//	[km/h]	Target vehicle speed.
		[ModalResultField(typeof(double))] acc,				//	[m/s2]	Vehicle acceleration.
        [ModalResultField(typeof(double))] grad,			//	[%]	    Road gradient.
		[ModalResultField(typeof(double))] Gear,			//	[-]	    Gear. "0" = clutch opened / neutral. "0.5" = lock-up clutch is open (AT with torque converter only, see Gearbox)
		[ModalResultField(typeof(double), "Ploss GB")] PlossGB,			//	[kW]	Gearbox losses.
		[ModalResultField(typeof(double), "Ploss Diff")] PlossDiff,		//	[kW]	Losses in differential / axle transmission.
		[ModalResultField(typeof(double), "Ploss Retarder")] PlossRetarder,	//	[kW]	Retarder losses.
		[ModalResultField(typeof(double), "Pa GB")] PaGB,			//	[kW]	Rotational acceleration power: Gearbox.
		[ModalResultField(typeof(double), "Pa Veh")] PaVeh,			//	[kW]	Vehicle acceleration power.
		[ModalResultField(typeof(double))] Proll,			//	[kW]	Rolling resistance power demand.
		[ModalResultField(typeof(double))] Pair,			//	[kW]	Air resistance power demand.
		[ModalResultField(typeof(double))] Pgrad,			//	[kW]	Power demand due to road gradient.
		[ModalResultField(typeof(double))] Pwheel,			//	[kW]	Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance.
		[ModalResultField(typeof(double))] Pbrake,			//	[kW]	Brake power. Drag power is included in Pe.
        //[ModalResultField(typeof(double))] Paux_xxx,		//	[kW]	Power demand of Auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
		[ModalResultField(typeof(double))] TCν,				//	[-]	    Torque converter speed ratio
		[ModalResultField(typeof(double), "TCµ")] TCmu,			//	[-]	    Torque converter torque ratio
		[ModalResultField(typeof(double))] TC_M_Out,		//	[Nm]	Torque converter output torque
		[ModalResultField(typeof(double))] TC_n_Out,		//	[1/min]	Torque converter output speed
    }


    [AttributeUsage(AttributeTargets.Field)]
    class ModalResultFieldAttribute : Attribute
    {
        internal ModalResultFieldAttribute(Type fieldType, string name = null)
        {
            FieldType = fieldType;
            Name = name;
        }
        public Type FieldType { get; private set; }
        public string Name { get; private set; }
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

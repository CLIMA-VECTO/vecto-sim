using System;
using System.Data;
using System.Reflection;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    [System.ComponentModel.DesignerCategory("")] // to disable design view in VisualStudio
    public class ModalResults : DataTable
    {
        public ModalResults()
        {
            foreach (ModalResultField value in Enum.GetValues(typeof(ModalResultField)))
                Columns.Add(value.ToString(), value.GetDataType());
        }

        public DataTable ReadFromFile(string fileName)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Enum with field definitions of the Modal Results File (.vmod).
    /// </summary>
    public enum ModalResultField
    {
        [ModalResultFieldAttr(typeof(double))] time,			//	[s]	    Time step.
        [ModalResultFieldAttr(typeof(double))] dist,			//	[km]	Travelled distance.
        [ModalResultFieldAttr(typeof(double))] v_act,			//	[km/h]	Actual vehicle speed.
		[ModalResultFieldAttr(typeof(double))] v_targ,			//	[km/h]	Target vehicle speed.
		[ModalResultFieldAttr(typeof(double))] acc,				//	[m/s2]	Vehicle acceleration.
        [ModalResultFieldAttr(typeof(double))] grad,			//	[%]	    Road gradient.
		[ModalResultFieldAttr(typeof(double))] n,			    //	[1/min]	Engine speed.
		[ModalResultFieldAttr(typeof(double))] Tq_eng,			//	[Nm]	Engine torque.
		[ModalResultFieldAttr(typeof(double))] Tq_clutch,		//	[Nm]	Torque at clutch (before clutch, engine-side)
		[ModalResultFieldAttr(typeof(double))] Tq_full,			//	[Nm]	Full load torque
		[ModalResultFieldAttr(typeof(double))] Tq_drag,			//	[Nm]	Motoring torque
		[ModalResultFieldAttr(typeof(double))] Pe_eng,			//	[kW]	Engine power.
		[ModalResultFieldAttr(typeof(double))] Pe_full,			//	[kW]	Engine full load power.
		[ModalResultFieldAttr(typeof(double))] Pe_drag,			//	[kW]	Engine drag power.
		[ModalResultFieldAttr(typeof(double))] Pe_clutch,		//	[kW]	Engine power at clutch (equals Pe minus loss due to rotational inertia Pa Eng).
		[ModalResultFieldAttr(typeof(double))] Gear,			//	[-]	    Gear. "0" = clutch opened / neutral. "0.5" = lock-up clutch is open (AT with torque converter only, see Gearbox)
		[ModalResultFieldAttr(typeof(double))] PlossGB,			//	[kW]	Gearbox losses.
		[ModalResultFieldAttr(typeof(double))] PlossDiff,		//	[kW]	Losses in differential / axle transmission.
		[ModalResultFieldAttr(typeof(double))] PlossRetarder,	//	[kW]	Retarder losses.
		[ModalResultFieldAttr(typeof(double))] PaEng,			//	[kW]	Rotational acceleration power: Engine.
		[ModalResultFieldAttr(typeof(double))] PaGB,			//	[kW]	Rotational acceleration power: Gearbox.
		[ModalResultFieldAttr(typeof(double))] PaVeh,			//	[kW]	Vehicle acceleration power.
		[ModalResultFieldAttr(typeof(double))] Proll,			//	[kW]	Rolling resistance power demand.
		[ModalResultFieldAttr(typeof(double))] Pair,			//	[kW]	Air resistance power demand.
		[ModalResultFieldAttr(typeof(double))] Pgrad,			//	[kW]	Power demand due to road gradient.
		[ModalResultFieldAttr(typeof(double))] Paux,			//	[kW]	Total auxiliary power demand .
		[ModalResultFieldAttr(typeof(double))] Pwheel,			//	[kW]	Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance.
		[ModalResultFieldAttr(typeof(double))] Pbrake,			//	[kW]	Brake power. Drag power is included in Pe.
		[ModalResultFieldAttr(typeof(double))] Paux_xxx,		//	[kW]	Power demand of Auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
		[ModalResultFieldAttr(typeof(double))] FC,			    //	[g/h]	Fuel consumption from FC map..
		[ModalResultFieldAttr(typeof(double))] FC_AUXc,			//	[g/h]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
		[ModalResultFieldAttr(typeof(double))] FC_WHTCc,		//	[g/h]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
		[ModalResultFieldAttr(typeof(double))] TCν,				//	[-]	    Torque converter speed ratio
		[ModalResultFieldAttr(typeof(double))] TCmu,			//	[-]	    Torque converter torque ratio
		[ModalResultFieldAttr(typeof(double))] TC_M_Out,		//	[Nm]	Torque converter output torque
		[ModalResultFieldAttr(typeof(double))] TC_n_Out,		//	[1/min]	Torque converter output speed
    }






	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	class ModalResultFieldAttr : Attribute
	{
		internal ModalResultFieldAttr(Type fieldType)
		{
			this.FieldType = fieldType;
		}
		public Type FieldType { get; private set; }
	}

    public static class ModalResultFieldExtensions
    {
        public static Type GetDataType(this ModalResultField field)
        {
            return GetAttr(field).FieldType;
        }

	    private static ModalResultFieldAttr GetAttr(ModalResultField field)
	    {
		    return (ModalResultFieldAttr)Attribute.GetCustomAttribute(ForValue(field), typeof (ModalResultFieldAttr));
	    }

	    private static MemberInfo ForValue(ModalResultField field)
	    {
		    return typeof (ModalResultField).GetField(Enum.GetName(typeof (ModalResultField), field));
	    }
    }
}

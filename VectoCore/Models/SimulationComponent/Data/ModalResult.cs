using System;
using System.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public class ModalResults : DataTable
    {
        public ModalResults()
        {
            foreach (ModalResult value in Enum.GetValues(typeof(ModalResult)))
                Columns.Add(value.ToString(), value.GetDataType());
        }
    }

    /// <summary>
    /// Enum with field definitions of the Modal Results File (.vmod).
    /// </summary>
    public enum ModalResult
    {
        time,			//	[s]	    Time step.
        dist,			//	[km]	Travelled distance.
        v_act,			//	[km/h]	Actual vehicle speed.
        v_targ,			//	[km/h]	Target vehicle speed.
        acc,			//	[m/s2]	Vehicle acceleration.
        grad,			//	[%]	    Road gradient.
        n,			    //	[1/min]	Engine speed.
        Tq_eng,			//	[Nm]	Engine torque.
        Tq_clutch,		//	[Nm]	Torque at clutch (before clutch, engine-side)
        Tq_full,		//	[Nm]	Full load torque
        Tq_drag,		//	[Nm]	Motoring torque
        Pe_eng,			//	[kW]	Engine power.
        Pe_full,		//	[kW]	Engine full load power.
        Pe_drag,		//	[kW]	Engine drag power.
        Pe_clutch,		//	[kW]	Engine power at clutch (equals Pe minus loss due to rotational inertia Pa Eng).
        Gear,			//	[-]	    Gear. "0" = clutch opened / neutral. "0.5" = lock-up clutch is open (AT with torque converter only, see Gearbox)
        PlossGB,		//	[kW]	Gearbox losses.
        PlossDiff,		//	[kW]	Losses in differential / axle transmission.
        PlossRetarder,	//	[kW]	Retarder losses.
        PaEng,			//	[kW]	Rotational acceleration power: Engine.
        PaGB,			//	[kW]	Rotational acceleration power: Gearbox.
        PaVeh,			//	[kW]	Vehicle acceleration power.
        Proll,			//	[kW]	Rolling resistance power demand.
        Pair,			//	[kW]	Air resistance power demand.
        Pgrad,			//	[kW]	Power demand due to road gradient.
        Paux,			//	[kW]	Total auxiliary power demand .
        Pwheel,			//	[kW]	Total power demand at wheel = sum of rolling, air, acceleration and road gradient resistance.
        Pbrake,			//	[kW]	Brake power. Drag power is included in Pe.
        Paux_xxx,		//	[kW]	Power demand of Auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
        FC,			    //	[g/h]	Fuel consumption from FC map..
        FC_AUXc,		//	[g/h]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
        FC_WHTCc,		//	[g/h]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
        TCν,			//	[-]	    Torque converter speed ratio
        TCmu,			//	[-]	    Torque converter torque ratio
        TC_M_Out,		//	[Nm]	Torque converter output torque
        TC_n_Out,		//	[1/min]	Torque converter output speed
    }

    public static class ModalResultFieldExtensions
    {
        public static Type GetDataType(this ModalResult field)
        {
            return typeof(double);
        }
    }
}

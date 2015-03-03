using System.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public static class ModalResult
    {
        public static DataTable getDataTable()
        {
            DataTable data = new DataTable();
            data.Columns.Add(ModalResultFields.time.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.dist.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.v_act.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.v_targ.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.acc.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.grad.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.n.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Tq_eng.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Tq_clutch.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Tq_full.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Tq_drag.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pe_eng.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pe_full.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pe_drag.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pe_clutch.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Gear.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PlossGB.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PlossDiff.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PlossRetarder.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PaEng.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PaGB.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.PaVeh.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Proll.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pair.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pgrad.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Paux.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pwheel.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Pbrake.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.Paux_xxx.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.FC.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.FC_AUXc.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.FC_WHTCc.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.TCν.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.TCμ.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.TC_M_Out.ToString(), typeof(float));
            data.Columns.Add(ModalResultFields.TC_n_Out.ToString(), typeof(float));
            return data;
        }
    }
}

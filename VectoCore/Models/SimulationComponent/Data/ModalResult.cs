using System.Data;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
    public static class ModalResult
    {
        public static DataTable getDataTable()
        {
            DataTable data = new DataTable();
            data.Columns.Add(ModalResultFields.time.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.dist.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.v_act.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.v_targ.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.acc.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.grad.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.n.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Tq_eng.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Tq_clutch.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Tq_full.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Tq_drag.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pe_eng.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pe_full.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pe_drag.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pe_clutch.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Gear.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PlossGB.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PlossDiff.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PlossRetarder.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PaEng.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PaGB.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.PaVeh.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Proll.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pair.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pgrad.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Paux.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pwheel.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Pbrake.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.Paux_xxx.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.FC.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.FC_AUXc.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.FC_WHTCc.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.TCν.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.TCμ.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.TC_M_Out.ToString(), typeof(double));
            data.Columns.Add(ModalResultFields.TC_n_Out.ToString(), typeof(double));
            return data;
        }

    }
}

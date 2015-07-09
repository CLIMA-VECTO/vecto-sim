using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class ModalDataWriter : IModalDataWriter
	{
		private readonly bool _engineOnly;
		private ModalResults Data { get; set; }
		private DataRow CurrentRow { get; set; }
		private string ModFileName { get; set; }


		public ModalDataWriter(string modFileName, bool engineOnly)
		{
			HasTorqueConverter = false;
			ModFileName = modFileName;
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
			_engineOnly = engineOnly;
		}

		public bool HasTorqueConverter { get; set; }

		public void CommitSimulationStep()
		{
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();
		}

		public void Finish()
		{
			var dataColumns = new List<ModalResultField> { ModalResultField.time };

			if (!_engineOnly) {
				dataColumns.AddRange(new[] {
					ModalResultField.time,
					ModalResultField.dist,
					ModalResultField.v_act,
					ModalResultField.v_targ,
					ModalResultField.acc,
					ModalResultField.grad
				});
			}

			dataColumns.AddRange(new[] {
				ModalResultField.n,
				ModalResultField.Tq_eng,
				ModalResultField.Tq_clutch,
				ModalResultField.Tq_full,
				ModalResultField.Tq_drag,
				ModalResultField.Pe_eng,
				ModalResultField.Pe_full,
				ModalResultField.Pe_drag,
				ModalResultField.Pe_clutch,
				ModalResultField.PaEng,
				ModalResultField.Paux
			});

			if (!_engineOnly) {
				dataColumns.AddRange(new[] {
					ModalResultField.Gear,
					ModalResultField.PlossGB,
					ModalResultField.PlossDiff,
					ModalResultField.PlossRetarder,
					ModalResultField.PaGB,
					ModalResultField.PaVeh,
					ModalResultField.Proll,
					ModalResultField.Pair,
					ModalResultField.Pgrad,
					ModalResultField.Pwheel,
					ModalResultField.Pbrake
				});

				if (HasTorqueConverter) {
					dataColumns.AddRange(new[] {
						ModalResultField.TCν,
						ModalResultField.TCmu,
						ModalResultField.TC_M_Out,
						ModalResultField.TC_n_Out
					});
				}

				//todo: auxiliaries
			}
			VectoCSVFile.Write(ModFileName, new DataView(Data).ToTable(false, dataColumns.Select(x => x.GetName()).ToArray()));
		}


		public object Compute(string expression, string filter)
		{
			return Data.Compute(expression, filter);
		}

		public IEnumerable<T> GetValues<T>(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x.Field<T>((int)key));
		}


		public object this[ModalResultField key]
		{
			get { return CurrentRow[(int)key]; }
			set { CurrentRow[(int)key] = value; }
		}
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Simulation.Impl;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
    /// <summary>
    /// Class for representation of one EngineOnly Driving Cycle
    /// </summary>
    /// <remarks>
    /// The driving cycle (.vdri) must contain:
    /// <n> Engine speed
    /// <Me>|<Pe> Engine torque or engine power at clutch.
    /// 
    /// Optional:
    /// <Padd> Additional power demand (aux) 
    /// 
    /// To explicitly define motoring operation use the <DRAG> keyword - VECTO replaces the keyword with the motoring torque/power from the .vfld file during calculation.
    /// </remarks>
    public class EngineOnlyDrivingCycle : VectoSimulationComponent, IDrivingCycle, ITnInPort
    {
        private static class Fields
        {
            public const string EngineSpeed = "n";
            public const string EnginePower = "Pe";
            public const string Torque = "Me";
            public const string AuxilariesPower = "Padd";
        }

        protected TimeSpan AbsTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
        protected TimeSpan dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);
        protected List<EngineOnlyDrivingCycleEntry> CycleEntries = new List<EngineOnlyDrivingCycleEntry>();

        private ITnOutPort OutPort { get; set; }

        private int CurrentStep { get; set; }

        public class EngineOnlyDrivingCycleEntry
        {
            public double EngineSpeed { get; set; }

            public double Torque { get; set; }

            public double AuxilariesPower { get; set; }

            //todo: handle drag mode when drag is set true!
            public bool Drag { get; set; }
        }

        #region IDrivingCycle
        public bool DoSimulationStep()
        {
            if (CycleEntries.Count >= CurrentStep)
                return false;

            var entry = CycleEntries[CurrentStep];
            OutPort.Request(AbsTime, dt, entry.Torque, entry.EngineSpeed);
            AbsTime += dt;
            CurrentStep++;
            return true;
        }
        #endregion

        #region ITnInPort
        public void Connect(ITnOutPort other)
        {
            OutPort = other;
        }
        #endregion

        #region IInShaft
        public ITnInPort InShaft()
        {
            return this;
        }
        #endregion

        public IReadOnlyList<EngineOnlyDrivingCycleEntry> Entries
        {
            get { return CycleEntries.AsReadOnly(); }
        }

        public EngineOnlyDrivingCycle(IVehicleContainer container, string fileName)
        {
            var data = VectoCSVFile.Read(fileName);

            DataColumn engineSpeedCol = null, torqueCol = null, powerCol = null, auxCol = null;

            if (HeaderIsValid(data))
            {
                engineSpeedCol = data.Columns[Fields.EngineSpeed];
                torqueCol = data.Columns[Fields.Torque];
                powerCol = data.Columns[Fields.EnginePower];
                auxCol = data.Columns[Fields.AuxilariesPower];
            }
            else
            {
                engineSpeedCol = data.Columns[0];
                torqueCol = data.Columns[1];
                if (data.Columns.Count > 2)
                    auxCol = data.Columns[2];
            }

            foreach (DataRow row in data.Rows)
            {
                var entry = new EngineOnlyDrivingCycleEntry();

                entry.EngineSpeed = row.GetDouble(engineSpeedCol);

                if (torqueCol != null)
                {
                    if (row.Field<string>(torqueCol).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.Torque = row.GetDouble(torqueCol);
                }
                else
                {
                    if (row.Field<string>(powerCol).Equals("<DRAG>"))
                        entry.Drag = true;
                    else
                        entry.Torque = VectoMath.ConvertPowerToTorque(row.GetDouble(powerCol), entry.EngineSpeed);
                }

                if (auxCol != null)
                    entry.AuxilariesPower = row.GetDouble(auxCol);

                CycleEntries.Add(entry);
            }
            container.AddComponent(this);
        }

        private static bool HeaderIsValid(DataTable data)
        {
            return (!(data.Columns[Fields.EngineSpeed] != null
                && (data.Columns[Fields.Torque] == null || data.Columns[Fields.EnginePower] == null)));
        }

        public override void CommitSimulationStep(IModalDataWriter writer)
        {

        }
    }
}

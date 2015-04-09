using System;
using System.Collections.Generic;
using System.Data;
using Common.Logging;
using Newtonsoft.Json;
using TUGraz.VectoCore.Models.Connector.Ports;
using TUGraz.VectoCore.Models.Connector.Ports.Impl;
using TUGraz.VectoCore.Models.Simulation.Data;
using TUGraz.VectoCore.Models.SimulationComponent;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Impl
{
    public class VectoJob
    {
        /// <summary>
        ///     A class which represents the json data format for serializing and deserializing the VectoJob files.
        /// </summary>
        /// <remarks>
        ///{
        ///   "Header": {
        ///     "CreatedBy": " ()",
        ///     "Date": "3/4/2015 2:09:13 PM",
        ///     "AppVersion": "2.0.4-beta3",
        ///     "FileVersion": 2
        ///   },
        ///   "Body": {
        ///     "SavedInDeclMode": false,
        ///     "VehicleFile": "24t Coach.vveh",
        ///     "EngineFile": "24t Coach.veng",
        ///     "GearboxFile": "24t Coach.vgbx",
        ///     "Cycles": [
        ///       "W:\\VECTO\\CITnet\\VECTO\\bin\\Debug\\Declaration\\MissionCycles\\LOT2_rural Engine Only.vdri"
        ///     ],
        ///     "Aux": [
        ///       {
        ///         "ID": "ALT1",
        ///         "Type": "Alternator",
        ///         "Path": "24t_Coach_ALT.vaux",
        ///         "Technology": ""
        ///       },
        ///       {
        ///         "ID": "ALT2",
        ///         "Type": "Alternator",
        ///         "Path": "24t_Coach_ALT.vaux",
        ///         "Technology": ""
        ///       },
        ///       {
        ///         "ID": "ALT3",
        ///         "Type": "Alternator",
        ///         "Path": "24t_Coach_ALT.vaux",
        ///         "Technology": ""
        ///       }
        ///     ],
        ///     "VACC": "Coach.vacc",
        ///     "EngineOnlyMode": true,
        ///     "StartStop": {
        ///       "Enabled": false,
        ///       "MaxSpeed": 5.0,
        ///       "MinTime": 0.0,
        ///       "Delay": 0
        ///     },
        ///     "LAC": {
        ///       "Enabled": true,
        ///       "Dec": -0.5,
        ///       "MinSpeed": 50.0
        ///     },
        ///     "OverSpeedEcoRoll": {
        ///       "Mode": "OverSpeed",
        ///       "MinSpeed": 70.0,
        ///       "OverSpeed": 5.0,
        ///       "UnderSpeed": 5.0
        ///     }
        ///   }
        /// }
        /// </remarks>
        public class Data
        {
            [JsonProperty(Required = Required.Always)] public DataHeader Header;
            [JsonProperty(Required = Required.Always)] public DataBody Body;

            public class DataHeader
            {
                [JsonProperty(Required = Required.Always)] public string CreatedBy;
                [JsonProperty(Required = Required.Always)] public DateTime Date;
                [JsonProperty(Required = Required.Always)] public string AppVersion;
                [JsonProperty(Required = Required.Always)] public double FileVersion;
            }

            public class DataBody
            {
                [JsonProperty("SavedInDeclMode")] public bool SavedInDeclarationMode;

                [JsonProperty(Required = Required.Always)] public string VehicleFile;
                [JsonProperty(Required = Required.Always)] public string EngineFile;
                [JsonProperty(Required = Required.Always)] public string GearboxFile;
                [JsonProperty(Required = Required.Always)] public IList<string> Cycles;
                [JsonProperty(Required = Required.Always)] public IList<AuxData> Aux;
                [JsonProperty(Required = Required.Always)] public string VACC;
                [JsonProperty(Required = Required.Always)] public bool EngineOnlyMode;
                [JsonProperty(Required = Required.Always)] public StartStopData StartStop;
                [JsonProperty(Required = Required.Always)] public LACData LAC;
                [JsonProperty(Required = Required.Always)] public OverSpeedEcoRollData OverSpeedEcoRoll;

                public class AuxData
                {
                    [JsonProperty(Required = Required.Always)] public string ID;
                    [JsonProperty(Required = Required.Always)] public string Type;
                    [JsonProperty(Required = Required.Always)] public string Path;
                    [JsonProperty(Required = Required.Always)] public string Technology;
                }

                public class StartStopData
                {
                    [JsonProperty(Required = Required.Always)] public bool Enabled;
                    [JsonProperty(Required = Required.Always)] public double MaxSpeed;
                    [JsonProperty(Required = Required.Always)] public double MinTime;
                    [JsonProperty(Required = Required.Always)] public double Delay;
                }

                public class LACData
                {
                    [JsonProperty(Required = Required.Always)] public bool Enabled;
                    [JsonProperty(Required = Required.Always)] public double Dec;
                    [JsonProperty(Required = Required.Always)] public double MinSpeed;
                }

                public class OverSpeedEcoRollData
                {
                    [JsonProperty(Required = Required.Always)] public string Mode;
                    [JsonProperty(Required = Required.Always)] public double MinSpeed;
                    [JsonProperty(Required = Required.Always)] public double OverSpeed;
                    [JsonProperty(Required = Required.Always)] public double UnderSpeed;
                }
            }
        }
    }


    /// <summary>
    /// Simulator for one vecto simulation job.
    /// </summary>
    public class VectoSimulator : IVectoSimulator
    {
        private TimeSpan _absTime = new TimeSpan(seconds: 0, minutes: 0, hours: 0);
        private TimeSpan _dt = new TimeSpan(seconds: 1, minutes: 0, hours: 0);

        public VectoSimulator(string name, string fileName, IVehicleContainer container, IDrivingCycle cycle,
            IModalDataWriter dataWriter)
        {
            Name = name;
            FileName = fileName;
            Container = container;
            Cycle = cycle;
            DataWriter = dataWriter;
        }

        public string FileName { get; set; }

        protected string Name { get; set; }

        protected IDrivingCycle Cycle { get; set; }
        protected IModalDataWriter DataWriter { get; set; }
        protected IVehicleContainer Container { get; set; }

        public IVehicleContainer GetContainer()
        {
            return Container;
        }

        public void Run()
        {
            LogManager.GetLogger(GetType()).Info("VectoJob started running.");
            IResponse response;
            do {
                response = Cycle.Request(_absTime, _dt);
                while (response is ResponseFailTimeInterval) {
                    _dt = (response as ResponseFailTimeInterval).DeltaT;
                    response = Cycle.Request(_absTime, _dt);
                }

                if (response is ResponseCycleFinished) {
                    break;
                }

                DataWriter[ModalResultField.time] = (_absTime + TimeSpan.FromTicks(_dt.Ticks / 2)).TotalSeconds;
                DataWriter[ModalResultField.simulationInterval] = _dt.TotalSeconds;

                Container.CommitSimulationStep(DataWriter);

                // set _dt to difference to next full second.
                _absTime += _dt;
                _dt = TimeSpan.FromSeconds(1) - TimeSpan.FromMilliseconds(_dt.Milliseconds);
            } while (response is ResponseSuccess);

            Container.FinishSimulation(DataWriter);


            //todo: WriteSummary();

            LogManager.GetLogger(GetType()).Info("VectoJob finished.");
        }


        /*
        Name	Unit	Description
        Job	[-]	Job number. Format is "x-y" with x = file number and y = cycle number
        Input File	[-]	Name of the input file
        Cycle	[-]	Name of the cycle file
        time	[s]	Total simulation time
        distance	[km]	Total travelled distance
        speed	[km/h]	Average vehicle speed
        ∆altitude	[m]	Altitude difference between start and end of cycle
        Ppos	[kW]	Average positive engine power
        Pneg	[kW]	Average negative engine power
        FC	[g/km]	Average fuel consumption
        FC-AUXc	[g/km]	Fuel consumption after Auxiliary-Start/Stop Correction. (Based on FC.)
        FC-WHTCc	[g/km]	Fuel consumption after WHTC Correction. (Based on FC-AUXc.)
        Pbrake	[kW]	Average brake power (not including engine drag)
        EposICE	[kWh]	Total positive engine work
        EnegICE	[kWh]	Total negative engine work (engine brake)
        Eair	[kWh]	Total work of air resistance
        Eroll	[kWh]	Total work of rolling resistance
        Egrad	[kWh]	Total work of gradient resistance
        Eacc	[kWh]	Total work from accelerations (<0) / decelerations (>0) 
        Eaux	[kWh]	Total energy demand of auxiliaries
        Eaux_xxx	[kWh]	Energy demand of auxiliary with ID xxx. See also Aux Dialog and Driving Cycle.
        Ebrake	[kWh]	Total work dissipated in mechanical braking (sum of service brakes, retader and additional engine exhaust brakes)
        Etransm	[kWh]	Total work of transmission losses
        Eretarder	[kWh]	Total retarder losses
        Mass	[kg]	Vehicle mass (equals Curb Weight Vehicle plus Curb Weight Extra Trailer/Body, see Vehicle Editor)
        Loading	[kg]	Vehicle loading (see Vehicle Editor)
        a	[m/s2]	Average acceleration
        a_pos	[m/s2]	Average acceleration in acceleration phases*
        a_neg	[m/s2]	Average deceleration in deceleration phases*
        Acc.Noise	[m/s2]	Acceleration noise
        pAcc	[%]	Time share of acceleration phases*
        pDec	[%]	Time share of deceleration phases*
        pCruise	[%]	Time share of cruise phases*
        pStop	[%]	Time share of stop phases*
        */

        private static class SummaryFile
        {
            private static DataTable table;

            static SummaryFile()
            {
                table = new DataTable();
                table.Columns.Add("Job [-]", typeof(string));
                table.Columns.Add("Input File [-]", typeof(string));
                table.Columns.Add("Cycle [-]", typeof(string));
                table.Columns.Add("Time [s]", typeof(double));
                table.Columns.Add("distance [km]", typeof(double));
                table.Columns.Add("speed [km/h]", typeof(double));
                table.Columns.Add("∆altitude [m]", typeof(double));
                table.Columns.Add("Ppos [kw]", typeof(double));
                table.Columns.Add("Pneg [kw]", typeof(double));
                table.Columns.Add("FC [g/km]", typeof(double));
                table.Columns.Add("FC-AUXc [g/km]", typeof(double));
                table.Columns.Add("FC-WHTCc [g/km]", typeof(double));
                table.Columns.Add("Pbrake [kw]", typeof(double));
                table.Columns.Add("EposICE [kwh]", typeof(double));
                table.Columns.Add("EnegICE [kwh]", typeof(double));
                table.Columns.Add("Eair [kwh]", typeof(double));
                table.Columns.Add("Eroll [kwh]", typeof(double));
                table.Columns.Add("Egrad [kwh]", typeof(double));
                table.Columns.Add("Eacc [kwh]", typeof(double));
                table.Columns.Add("Eaux [kwh]", typeof(double));
                table.Columns.Add("Eaux_xxx [kwh]", typeof(double));
                table.Columns.Add("Ebrake [kwh]", typeof(double));
                table.Columns.Add("Etransm [kwh]", typeof(double));
                table.Columns.Add("Eretarder [kwh]", typeof(double));
                table.Columns.Add("Mass [kg]", typeof(double));
                table.Columns.Add("Loading [kg]", typeof(double));
                table.Columns.Add("a [m/s2]", typeof(double));
                table.Columns.Add("a_pos [m/s2]", typeof(double));
                table.Columns.Add("a_neg [m/s2]", typeof(double));
                table.Columns.Add("Acc.Noise [m/s2]", typeof(double));
                table.Columns.Add("pAcc [%]", typeof(double));
                table.Columns.Add("pDec [%]", typeof(double));
                table.Columns.Add("pCruise [%]", typeof(double));
                table.Columns.Add("pStop [%]", typeof(double));
            }


            private void WriteSummary(ModalResults modData)
            {
                //var data = new ModalResults();

                var row = table.NewRow();
                //row["Job [-]"] = Name;
                //row["Input File [-]"] = FileName;
                //row["Cycle [-]"] = Container.CycleFileName();
                //row["time [s]"] = data.Compute("Max(time)", "");
                //row["distance [km]"] = data.Compute("Max(distance)", "");
                //row["speed [km/h]"] = data.Compute("Avg(speed)", "");
                //row["∆altitude [m]"] = data.Rows[data.Rows.Count-1].Field<double>("altitude") - data.Rows[0].Field<double>("altitude");
                //row["Ppos [kw]"] = data.Compute("Avg(Peng)", "Peng > 0");
                //row["Pneg [kw]"] = data.Compute("Avg(Peng)", "Peng < 0");
                //row["FC [g/km]"] = data.Compute("Avg(FC)", "");
                //row["FC-AUXc [g/km]"] = data.Compute("Avg(FC-AUXc)", "");
                //row["FC-WHTCc [g/km]"] = data.Compute("Avg(FC-WHTCc)", "");
                //row["Pbrake [kw]"] = data.Compute("Avg(Pbrake)", "");
                //row["EposICE [kwh]"] = data.Compute("Avg(pos)", "pos > 0");
                //row["EnegICE [kwh]"] = data.Compute("Avg(pos)", "pos < 0");
                //row["Eair [kwh]"] = data.Compute("Sum(air)", "");
                //row["Eroll [kwh]"] = data.Compute("Sum(roll)", "");
                //row["Egrad [kwh]"] = data.Compute("Sum(grad)", "");
                //row["Eacc [kwh]"] = data.Compute("Sum(acc)", "");
                //row["Eaux [kwh]"] = data.Compute("Sum(aux)", "");

                //todo auxiliaries
                //foreach (var auxCol in data.Auxiliaries) {
                //    row["Eaux_" + auxCol.Name + " [kwh]"] = data.Compute("Sum(aux_" + auxCol.Name + ")", "");
                //}


                //row["Ebrake [kwh]"] = data.Compute("Sum(brake)", "");
                //row["Etransm [kwh]"] = data.Compute("Sum(transm)", "");
                //row["Eretarder [kwh]"] = data.Compute("Sum(retarder)", "");
                //row["Mass [kg]"] = Container.VehicleMass();
                //row["Loading [kg]"] = Container.LoadingMass();
                //row["a [m/s2]"] = data.Compute("Avg(a)", "");

                ////todo: a3s = average over 3 seconds!!!
                //row["a_pos [m/s2]"] = data.Compute("Avg(a)", "a > 0.125");
                //row["a_neg [m/s2]"] = data.Compute("Avg(a)", "a < -0.125");

                //// todo: is this really acc.noise?
                //row["Acc.Noise [m/s2]"] = data.Compute("Sum(a)", "a < 0.125 and a > -0.125");

                //row["pAcc [%]"] = (double) data.Compute("Sum(time_interval)", "a > 0.125") /
                //                  (double) data.Compute("Sum(time_interval)", "");
                //row["pDec [%]"] = (double) data.Compute("Sum(time_interval)", "a > 0.125") /
                //                  (double) data.Compute("Sum(time_interval)", "");
                //row["pCruise [%]"] = (double) data.Compute("Sum(time_interval)", "a > 0.125") /
                //                     (double) data.Compute("Sum(time_interval)", "");
                //row["pStop [%]"] = (double) data.Compute("Sum(time_interval)", "a < 0.125 and a > -0.125") /
                //                   (double) data.Compute("Sum(time_interval)", "");

                table.ImportRow(row);
                VectoCSVFile.Write(FileName, table);
            }
        }
    }
}
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public class ModalDataWriter : IModalDataWriter
	{
		private ModalResults Data { get; set; }
		private DataRow CurrentRow { get; set; }
		private string ModFileName { get; set; }


		public ModalDataWriter(string modFileName)
		{
			ModFileName = modFileName;
			Data = new ModalResults();
			CurrentRow = Data.NewRow();
		}

		public void CommitSimulationStep()
		{
			Data.Rows.Add(CurrentRow);
			CurrentRow = Data.NewRow();
		}

		public void Finish()
		{
			VectoCSVFile.Write(ModFileName, Data);
		}

		public object Compute(string expression, string filter)
		{
			return Data.Compute(expression, filter);
		}

		public IEnumerable<object> GetValues(ModalResultField key)
		{
			return Data.Rows.Cast<DataRow>().Select(x => x[(int)key]);
		}


		public object this[ModalResultField key]
		{
			get { return CurrentRow[(int)key]; }
			set { CurrentRow[(int)key] = value; }
		}
	}


	/*
	jobName	Unit	Description
	Job	[-]	Job number. Format is "x-y" with x = file number and y = cycle number
	Input File	[-]	jobName of the input file
	Cycle	[-]	jobName of the cycle file
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
}
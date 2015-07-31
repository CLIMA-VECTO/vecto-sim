using System;
using System.Collections.Generic;
using TUGraz.VectoCore.Exceptions;
using TUGraz.VectoCore.Models.Declaration;
using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.SimulationComponent.Data
{
	public class VehicleData : SimulationComponentData
	{
		public string BasePath { get; internal set; }


		public VehicleCategory VehicleCategory { get; internal set; }

		public VehicleClass VehicleClass { get; internal set; }

		public CrossWindCorrectionMode CrossWindCorrectionMode { get; internal set; }

		public RetarderData Retarder { get; internal set; }

		private List<Axle> _axleData;

		/// <summary>
		/// Set the properties for all axles of the vehicle
		/// </summary>
		public List<Axle> AxleData
		{
			get { return _axleData; }
			internal set
			{
				_axleData = value;
				ComputeRollResistanceAndReducedMassWheels();
			}
		}

		public AxleConfiguration AxleConfiguration { get; internal set; }

		public Kilogram CurbWeight { get; internal set; }

		public Kilogram CurbWeigthExtra { get; internal set; }

		public Kilogram Loading { get; internal set; }

		public Kilogram TotalVehicleWeight()
		{
			var retVal = 0.SI<Kilogram>();
			retVal += CurbWeight ?? 0.SI<Kilogram>();
			retVal += CurbWeigthExtra ?? 0.SI<Kilogram>();
			retVal += Loading ?? 0.SI<Kilogram>();
			return retVal;
		}

		public Kilogram TotalCurbWeight()
		{
			var retVal = 0.SI<Kilogram>();
			retVal += CurbWeight ?? 0.SI<Kilogram>();
			retVal += CurbWeigthExtra ?? 0.SI<Kilogram>();
			return retVal;
		}

		public Kilogram GrossVehicleMassRating { get; internal set; }

		public double DragCoefficient { get; internal set; }

		public SquareMeter CrossSectionArea { get; internal set; }

		public double DragCoefficientRigidTruck { get; internal set; }

		public SquareMeter CrossSectionAreaRigidTruck { get; internal set; }

		public Meter DynamicTyreRadius { get; internal set; }

		public Kilogram ReducedMassWheels { get; private set; }

		public string Rim { get; internal set; }

		public double TotalRollResistanceCoefficient { get; private set; }

		protected void ComputeRollResistanceAndReducedMassWheels()
		{
			if (TotalVehicleWeight() == 0.SI<Kilogram>()) {
				throw new VectoException("Total vehicle weight must be greater than 0! Set CurbWeight and Loading before!");
			}
			if (DynamicTyreRadius == null) {
				throw new VectoException("Dynamic tyre radius must be set before axles!");
			}

			var RRC = 0.0;
			var mRed0 = 0.SI<Kilogram>();
			foreach (var axle in _axleData) {
				var nrWheels = axle.TwinTyres ? 4 : 2;
				RRC += axle.AxleWeightShare * axle.RollResistanceCoefficient *
						Math.Pow(
							(axle.AxleWeightShare * TotalVehicleWeight() * Physics.GravityAccelleration / axle.TyreTestLoad /
							nrWheels).Value(), Physics.RollResistanceExponent - 1);
				mRed0 += nrWheels * (axle.Inertia / DynamicTyreRadius / DynamicTyreRadius).Cast<Kilogram>();
			}
			TotalRollResistanceCoefficient = RRC;
			ReducedMassWheels = mRed0;
		}
	}
}
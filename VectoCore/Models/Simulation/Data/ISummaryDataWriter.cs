using TUGraz.VectoCore.Utils;

namespace TUGraz.VectoCore.Models.Simulation.Data
{
	/// <summary>
	/// Provides methods to write and finish the Summary.
	/// </summary>
	public interface ISummaryDataWriter
	{
		/// <summary>
		/// Writes a single sum entry for the given modaldata of the job.
		/// </summary>
		/// <param name="data">The modal data.</param>
		/// <param name="vehicleMass">The vehicle mass.</param>
		/// <param name="vehicleLoading">The vehicle loading.</param>
		void Write(IModalDataWriter data, Kilogram vehicleMass = null, Kilogram vehicleLoading = null);

		/// <summary>
		/// Writes the data to the sum file.
		/// </summary>
		void Finish();
	}
}
namespace TUGraz.VectoCore.Models.Simulation.Data
{
	public interface ISummaryDataWriter
	{
		/// <summary>
		/// Writes a single sum entry for the given modaldata of the job.
		/// </summary>
		/// <param name="data">The modal data.</param>
		/// <param name="jobFileName">Name of the job file.</param>
		/// <param name="jobName">Name of the job.</param>
		/// <param name="cycleFileName">Name of the cycle file.</param>
		void Write(IModalDataWriter data, string jobFileName, string jobName, string cycleFileName);

		/// <summary>
		/// Writes the data to the sum file.
		/// </summary>
		void Finish();
	}
}
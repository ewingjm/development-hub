namespace Capgemini.DevelopmentHub.Develop.BusinessLogic
{
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.Model;

    /// <summary>
    /// Imports solutions.
    /// </summary>
    public interface IODataSolutionService
    {
        /// <summary>
        /// Import a solution zip file.
        /// </summary>
        /// <param name="solutionZip">The solution zip file.</param>
        /// <returns>The import job data.</returns>
        Task<ImportJobData> ImportSolutionZip(byte[] solutionZip);

        /// <summary>
        /// Merge solution components from a source solution into a target solution.
        /// </summary>
        /// <param name="sourceSolutionUniqueName">The unique name of the source solution.</param>
        /// <param name="targetSolutionUniqueName">The unique name of the target solution.</param>
        /// <param name="deleteSourceSolutionAfterMerge">Whether to to delete the source solution after merging.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task MergeSolutionComponents(string sourceSolutionUniqueName, string targetSolutionUniqueName, bool deleteSourceSolutionAfterMerge);
    }
}

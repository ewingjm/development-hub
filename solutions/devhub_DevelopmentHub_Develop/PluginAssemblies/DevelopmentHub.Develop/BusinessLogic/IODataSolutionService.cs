namespace DevelopmentHub.Develop.BusinessLogic
{
    using System.Threading.Tasks;
    using DevelopmentHub.Develop.Model;

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
        Task<ImportJobData> ImportSolutionZipAsync(byte[] solutionZip);

        /// <summary>
        /// Merge solution components from a source solution into a target solution.
        /// </summary>
        /// <param name="sourceSolutionUniqueName">The unique name of the source solution.</param>
        /// <param name="targetSolutionUniqueName">The unique name of the target solution.</param>
        /// <param name="deleteSourceSolutionAfterMerge">Whether to to delete the source solution after merging.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task MergeSolutionComponentsAsync(string sourceSolutionUniqueName, string targetSolutionUniqueName, bool deleteSourceSolutionAfterMerge);

        /// <summary>
        /// Export a solution zip file.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="isManaged">Whether or not the exported solution should be managed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<string> GetSolutionZipAsync(string solutionUniqueName, bool isManaged);

        /// <summary>
        /// Get a solution by it's uniquen name.
        /// </summary>
        /// <param name="uniqueName">The unique name of the solution.</param>
        /// <param name="fields">The fields to select.</param>
        /// <returns>The solution.</returns>
        Task<Model.OData.Solution> GetSolutionByUniqueNameAsync(string uniqueName, string[] fields);

        /// <summary>
        /// Updates the version of a solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution to update.</param>
        /// <param name="solutionVersion">The version.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateSolutionVersionAsync(string solutionUniqueName, string solutionVersion);
    }
}

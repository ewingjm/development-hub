namespace Capgemini.DevelopmentHub.Develop.BusinessLogic
{
    using System.Threading.Tasks;
    using Capgemini.DevelopmentHub.Develop.Model;

    /// <summary>
    /// Imports solutions.
    /// </summary>
    public interface ISolutionImportService
    {
        /// <summary>
        /// Import a solution zip file.
        /// </summary>
        /// <param name="solutionZip">The solution zip file.</param>
        /// <returns>The import job data.</returns>
        Task<ImportJobData> ImportSolutionZip(byte[] solutionZip);
    }
}

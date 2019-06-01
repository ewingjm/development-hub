namespace Capgemini.DevelopmentHub.Develop.BusinessLogic
{
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Facade for solution logic.
    /// </summary>
    public interface ISolutionService
    {
        /// <summary>
        /// Create a solution by unique name, display name and publisher.
        /// </summary>
        /// <param name="uniqueName">The unique name.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="description">The description.</param>
        /// <returns>A reference to the created solution.</returns>
        EntityReference Create(string uniqueName, string displayName, string description);

        /// <summary>
        /// Get the publisher for a solution.
        /// </summary>
        /// <param name="uniqueName">The unique name.</param>
        /// <returns>A reference to the publisher.</returns>
        EntityReference GetSolutionPublisher(string uniqueName);
    }
}

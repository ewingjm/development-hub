namespace DevelopmentHub.Develop.Model.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an ExportSolution request.
    /// </summary>
    [DataContract]
    public class ExportSolutionRequest
    {
        /// <summary>
        /// Gets or sets the unique name of the solution.
        /// </summary>
        [DataMember]
        public string SolutionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the solution should be exported as a managed solution.
        /// </summary>
        [DataMember]
        public bool Managed { get; set; }
    }
}

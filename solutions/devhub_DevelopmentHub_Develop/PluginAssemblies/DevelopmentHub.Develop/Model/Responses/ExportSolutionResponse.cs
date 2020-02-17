namespace DevelopmentHub.Develop.Model.Requests
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an ExportSolution response.
    /// </summary>
    [DataContract]
    public class ExportSolutionResponse
    {
        /// <summary>
        /// Gets or sets the compressed file that represents the exported solution.
        /// </summary>
        [DataMember]
        public string ExportSolutionFile { get; set; }
    }
}

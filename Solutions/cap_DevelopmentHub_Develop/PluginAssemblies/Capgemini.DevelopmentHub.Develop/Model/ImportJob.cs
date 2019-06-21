namespace Capgemini.DevelopmentHub.Develop.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contract for an import job.
    /// </summary>
    [DataContract]
    public class ImportJob
    {
        /// <summary>
        /// Gets or sets the solution ID.
        /// </summary>
        [DataMember(Name = "solutionid")]
        public Guid SolutionId { get; set; }

        /// <summary>
        /// Gets or sets the import job ID.
        /// </summary>
        [DataMember(Name = "importjobid")]
        public Guid ImportJobId { get; set; }

        /// <summary>
        /// Gets or sets the XML data.
        /// </summary>
        [DataMember(Name = "data")]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the solution name.
        /// </summary>
        [DataMember(Name = "solutionname")]
        public string SolutionName { get; set; }
    }
}

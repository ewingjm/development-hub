namespace Capgemini.DevelopmentHub.Develop.Model.OData
{
    using System;
    using System.Runtime.Serialization;
    using Capgemini.DevelopmentHub.Model;

    /// <summary>
    /// Data contract for an import job.
    /// </summary>
    [DataContract]
    public class ImportJob : ODataEntity
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

        /// <inheritdoc/>
        public override string EntitySet => "importjobs";

        /// <inheritdoc/>
        public override Guid EntityId => this.ImportJobId;
    }
}

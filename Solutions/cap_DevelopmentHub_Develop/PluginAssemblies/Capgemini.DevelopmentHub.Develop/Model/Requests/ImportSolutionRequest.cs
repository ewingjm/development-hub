#pragma warning disable CA1819 // Properties should not return arrays
namespace Capgemini.DevelopmentHub.Develop.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data contact for a solution import request.
    /// </summary>
    [DataContract]
    public class ImportSolutionRequest
    {
        /// <summary>
        /// Gets or sets a value indicating whether whether or not to overwrite unmanaged customisations.
        /// </summary>
        [DataMember]
        public bool OverwriteUnmanagedCustomizations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether or not to publish workflows.
        /// </summary>
        [DataMember]
        public bool PublishWorkflows { get; set; }

        /// <summary>
        /// Gets or sets the solution zip.
        /// </summary>
        [DataMember]
        public string CustomizationFile { get; set; }

        /// <summary>
        /// Gets or sets the import job ID.
        /// </summary>
        [DataMember]
        public Guid ImportJobId { get; set; }
    }
}

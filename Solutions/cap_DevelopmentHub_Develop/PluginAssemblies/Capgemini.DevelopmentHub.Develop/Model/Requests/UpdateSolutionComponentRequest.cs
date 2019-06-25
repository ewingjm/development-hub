namespace Capgemini.DevelopmentHub.Develop.Model.Requests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Updates a component in an unmanaged solution.
    /// </summary>
    [DataContract]
    public class UpdateSolutionComponentRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSolutionComponentRequest"/> class.
        /// </summary>
        /// <param name="componentId">The ID of the solution component.</param>
        /// <param name="componentType">The type of solution component.</param>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="includedComponentSettingsValues">Any settings to be included with the component.</param>
        public UpdateSolutionComponentRequest(Guid componentId, int componentType, string solutionUniqueName, string[] includedComponentSettingsValues)
        {
            this.ComponentId = componentId;
            this.ComponentType = componentType;
            this.SolutionUniqueName = solutionUniqueName;
            this.IncludedComponentSettingsValues = includedComponentSettingsValues;
        }

        /// <summary>
        /// Gets or sets the ID of the solution component.
        /// </summary>
        [DataMember]
        public Guid ComponentId { get; set; }

        /// <summary>
        /// Gets or sets the type of solution component to add to the unmanaged solution.
        /// </summary>
        [DataMember]
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets unique name of the solution.
        /// </summary>
        [DataMember]
        public string SolutionUniqueName { get; set; }

        /// <summary>
        /// Gets or sets any settings to be included with the component.
        /// When set to null, the component is added to the solution with metadata; otherwise passing an empty array results in no metadata settings included with the component.
        /// </summary>
        [DataMember]
        public IEnumerable<string> IncludedComponentSettingsValues { get; set; }
    }
}

namespace DevelopmentHub.Develop.Model.OData
{
    using System;
    using System.Runtime.Serialization;
    using DevelopmentHub.Model;

    /// <summary>
    /// A Dynamics 365 solution component.
    /// </summary>
    [DataContract]
    public class SolutionComponent : ODataEntity
    {
        /// <summary>
        /// Gets or sets the solution component ID.
        /// </summary>
        [DataMember(Name = "solutioncomponentid")]
        public Guid SolutionComponentId { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of the object with which the component is associated.
        /// </summary>
        [DataMember(Name = "objectid")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the type of solution component.
        /// </summary>
        [DataMember(Name = "componenttype")]
        public int ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the behaviour of the root component.
        /// </summary>
        [DataMember(Name = "rootcomponentbehavior")]
        public int? RootComponentBehavior { get; set; }

        /// <inheritdoc/>
        public override string EntitySet => "solutioncomponents";

        /// <inheritdoc/>
        public override Guid EntityId => this.SolutionComponentId;
    }
}

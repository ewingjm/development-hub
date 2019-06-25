namespace Capgemini.DevelopmentHub.Develop.Model.OData
{
    using System;
    using System.Runtime.Serialization;
    using Capgemini.DevelopmentHub.Model;

    /// <summary>
    /// A solution which contains CRM customizations.
    /// </summary>
    [DataContract]
    public class Solution : ODataEntity
    {
        /// <summary>
        /// Gets or sets the solution ID.
        /// </summary>
        [DataMember(Name = "solutionid")]
        public Guid SolutionId { get; set; }

        /// <inheritdoc/>
        public override string EntitySet => "solutions";

        /// <inheritdoc/>
        public override Guid EntityId => this.SolutionId;
    }
}

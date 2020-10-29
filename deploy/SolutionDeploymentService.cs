namespace DevelopmentHub.Deployment
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xrm.Sdk.Query;
    using Microsoft.Xrm.Tooling.Connector;
    using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

    /// <summary>
    /// Deployment functionality for solutions.
    /// </summary>
    public class SolutionDeploymentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionDeploymentService"/> class.
        /// </summary>
        /// <param name="crmServiceClient">A service client authenticated as a licensed user.</param>
        /// <param name="packageLog">The logger.</param>
        public SolutionDeploymentService(CrmServiceClient crmServiceClient, TraceLogger packageLog)
        {
            this.CrmSvc = crmServiceClient ?? throw new ArgumentNullException(nameof(crmServiceClient));
            this.PackageLog = packageLog ?? throw new ArgumentNullException(nameof(packageLog));
        }

        /// <summary>
        /// Gets a service client authenticated as a licensed user.
        /// </summary>
        protected CrmServiceClient CrmSvc { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected TraceLogger PackageLog { get; private set; }

        /// <summary>
        /// Returns the IDs of all solution components of a particular type.
        /// </summary>
        /// <param name="solutionId">The ID of the solution.</param>
        /// <param name="componentType">The type of component.</param>
        /// <returns>A collection of solution component IDs.</returns>
        public IEnumerable<Guid> GetSolutionComponentObjectIdsByType(Guid solutionId, int componentType)
        {
            this.PackageLog.Log($"Getting solution components of type {componentType} for solution {solutionId}.");

            var queryExpression = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid"),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            queryExpression.Criteria.AddCondition("componenttype", ConditionOperator.Equal, componentType);
            queryExpression.Criteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);

            var results = this.CrmSvc.RetrieveMultiple(queryExpression);

            this.PackageLog.Log($"Found {results.Entities.Count} matching components.");

            return results.Entities.Select(e => e.GetAttributeValue<Guid>("objectid")).ToArray();
        }

        /// <summary>
        /// Gets the ID of installed solution by unique name.
        /// </summary>
        /// <param name="solution">The solution unique name.</param>
        /// <returns>The ID of the solution (if found).</returns>
        public Guid? GetSolutionIdByUniqueName(string solution)
        {
            this.PackageLog.Log($"Getting solution ID for solution {solution}.");

            if (string.IsNullOrEmpty(solution))
            {
                throw new ArgumentException("You must provide a solution unique name.", nameof(solution));
            }

            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet(false),
                Criteria = new FilterExpression(LogicalOperator.And),
            };
            query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solution);

            var result = this.CrmSvc.RetrieveMultiple(query).Entities.FirstOrDefault()?.Id;
            this.PackageLog.Log($"Solution ID: {result}.");

            return result;
        }
    }
}

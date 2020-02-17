namespace DevelopmentHub.Repositories
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Extensions to <see cref="IOrganizationService"/>.
    /// </summary>
    public static class OrganizationServiceExtensions
    {
        /// <summary>
        /// Create a new <see cref="OrganizationServiceContext"/>.
        /// </summary>
        /// <typeparam name="TCrmContext">The type of <see cref="OrganizationServiceContext"/>.</typeparam>
        /// <param name="orgService">Organization service.</param>
        /// <returns>A new context.</returns>
        public static TCrmContext CreateNewCrmContext<TCrmContext>(this IOrganizationService orgService)
            where TCrmContext : OrganizationServiceContext
        {
            var context = (TCrmContext)Activator.CreateInstance(typeof(TCrmContext), orgService);
            context.MergeOption = MergeOption.NoTracking;
            context.SaveChangesDefaultOptions = SaveChangesOptions.None;
            return context;
        }

        /// <summary>
        /// Retrive multiple records by column value.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="entityName">The entity name.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnValue">The value to filter on.</param>
        /// <param name="columnsToRetrieve">The columns to select.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A collection of records meeting the criteria.</returns>
        public static EntityCollection GetEntitiesByColumn(this IOrganizationService orgService, string entityName, string columnName, object columnValue, ColumnSet columnsToRetrieve, int pageSize = 100)
        {
            var query = new QueryExpression(entityName)
            {
                ColumnSet = columnsToRetrieve,
            };

            if (!string.IsNullOrWhiteSpace(columnName) && columnValue != null)
            {
                query.Criteria.AddCondition(columnName, ConditionOperator.Equal, columnValue);
            }
            else if (!string.IsNullOrWhiteSpace(columnName) && columnValue == null)
            {
                query.Criteria.AddCondition(columnName, ConditionOperator.Null);
            }

            return orgService.GetDataByQuery(query, pageSize);
        }

        /// <summary>
        /// Get data by query expression.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="query">The query expression.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="shouldIncudeEntityCollection">Whether or not to include the entities in the response.</param>
        /// <returns>An entity collection of records matching the query.</returns>
        public static EntityCollection GetDataByQuery(this IOrganizationService orgService, QueryExpression query, int pageSize, bool shouldIncudeEntityCollection = true)
        {
            var allResults = new EntityCollection();

            query.PageInfo = new PagingInfo
            {
                Count = pageSize,
                PageNumber = 1,
                PagingCookie = null,
            };

            while (true)
            {
                var pagedResults = orgService.RetrieveMultiple(query);

                if (shouldIncudeEntityCollection)
                {
                    if (query.PageInfo.PageNumber == 1)
                    {
                        allResults = pagedResults;
                    }
                    else
                    {
                        allResults.Entities.AddRange(pagedResults.Entities);
                    }
                }
                else
                {
                    allResults.TotalRecordCount = allResults.TotalRecordCount + pagedResults.Entities.Count;
                }

                if (pagedResults.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = pagedResults.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return allResults;
        }

        /// <summary>
        /// Execute multiple requests.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="requests">The requests to execute.</param>
        /// <returns>The responses.</returns>
        public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService orgService, OrganizationRequestCollection requests)
        {
            var requestWithResults = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true,
                },
                Requests = new OrganizationRequestCollection(),
            };

            requestWithResults.Requests = requests;

            var responseWithResults = (ExecuteMultipleResponse)orgService.Execute(requestWithResults);

            return responseWithResults;
        }
    }
}

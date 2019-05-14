namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public static class OrganizationServiceExtensions
    {
        public static TCrmContext CreateNewCrmContext<TCrmContext>(this IOrganizationService service)
            where TCrmContext : OrganizationServiceContext
        {
            var context = (TCrmContext)Activator.CreateInstance(typeof(TCrmContext), service);
            context.MergeOption = MergeOption.NoTracking;
            context.SaveChangesDefaultOptions = SaveChangesOptions.None;
            return context;
        }

        public static EntityCollection GetEntitiesByColumn(this IOrganizationService orgService, string entityName, string columnName, object columnValue, string[] columnsToRetrieve = null, int pageSize = 100)
        {
            var query = new QueryExpression(entityName)
            {
                ColumnSet = columnsToRetrieve != null ? new ColumnSet(columnsToRetrieve) : new ColumnSet(true)
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

        public static EntityCollection GetDataByQuery(this IOrganizationService orgService, QueryExpression query, int pageSize, bool shouldIncudeEntityCollection = true)
        {
            var allResults = new EntityCollection();

            query.PageInfo = new PagingInfo
            {
                Count = pageSize,
                PageNumber = 1,
                PagingCookie = null
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

        public static ExecuteMultipleResponse ExecuteMultiple(this IOrganizationService orgService, OrganizationRequestCollection orgRequests)
        {
            var requestWithResults = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = new OrganizationRequestCollection()
            };

            requestWithResults.Requests = orgRequests;

            var responseWithResults = (ExecuteMultipleResponse)orgService.Execute(requestWithResults);

            return responseWithResults;
        }
    }
}

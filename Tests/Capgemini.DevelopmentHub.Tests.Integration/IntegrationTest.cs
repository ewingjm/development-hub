namespace Capgemini.DevelopmentHub.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Tooling.Connector;

    [ExcludeFromCodeCoverage]
    public abstract class IntegrationTest : IDisposable
    {
        private readonly List<EntityReference> createdEntities;

        public IntegrationTest()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.createdEntities = new List<EntityReference>();
            this.CrmServiceClient = new CrmServiceClient(Environment.GetEnvironmentVariable("CAKE_CONN_DEV"));
        }

        protected CrmServiceClient CrmServiceClient { get; private set; }

        protected IOrganizationService OrgService => this.CrmServiceClient.OrganizationServiceProxy;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.createdEntities?.Count > 0)
                {
                    this.DeleteTestData();
                }

                if (this.CrmServiceClient != null)
                {
                    this.CrmServiceClient.Dispose();
                    this.CrmServiceClient = null;
                }
            }
        }

        protected EntityReference CreateTestRecord(Entity record)
        {
            record.Id = this.CrmServiceClient.Create(record);
            var reference = record.ToEntityReference();
            this.createdEntities.Add(reference);

            return reference;
        }

        protected EntityReference[] CreateTestData(params Entity[] testData)
        {
            var requestCollection = new OrganizationRequestCollection();
            var createRequests = testData.Select(t => new CreateRequest { Target = t }).ToArray();
            requestCollection.AddRange(createRequests);

            var response = (ExecuteMultipleResponse)this.CrmServiceClient.Execute(new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                Requests = requestCollection
            });

            var entityReferences = new List<EntityReference>();
            for (int i = 0; i < response.Responses.Count; i++)
            {
                if (response.Responses[i].Fault != null)
                {
                    throw new Exception(response.Responses[i].Fault.Message);
                }

                var entityReference = new EntityReference(
                        createRequests[i].Target.LogicalName,
                        ((CreateResponse)response.Responses.ElementAt(i).Response).id);
                this.createdEntities.Add(entityReference);
                entityReferences.Add(entityReference);
            }

            return entityReferences.ToArray();
        }

        private void DeleteTestData()
        {
            var deleteRequests = new OrganizationRequestCollection();

            if (this.createdEntities.Count == 0)
            {
                return;
            }

            deleteRequests.AddRange(
                this.createdEntities.Select(
                    e => new DeleteRequest { Target = e }));

            this.CrmServiceClient.Execute(new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                },
                Requests = deleteRequests
            });
        }
    }
}

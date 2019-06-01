namespace Capgemini.DevelopmentHub.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using Capgemini.DevelopmentHub.BusinessLogic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Tooling.Connector;

    /// <summary>
    /// Base integration test class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class IntegrationTest : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
        /// </summary>
        /// <param name="developmentUrl">The URL of the development environment.</param>
        /// <param name="username">Username used to log in to the development environment.</param>
        public IntegrationTest(Uri developmentUrl, string username = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.CreatedEntities = new List<EntityReference>();
            this.CrmServiceClient = new CrmServiceClient(this.GetConnectionString(developmentUrl, username));
            this.RepositoryFactory = new RepositoryFactory(this.OrgService);
        }

        /// <summary>
        /// Gets the created entities.
        /// </summary>
        protected IList<EntityReference> CreatedEntities { get; private set; }

        /// <summary>
        /// Gets the service client.
        /// </summary>
        protected CrmServiceClient CrmServiceClient { get; private set; }

        /// <summary>
        /// Gets the Organization Service.
        /// </summary>
        protected IOrganizationService OrgService => this.CrmServiceClient.OrganizationServiceProxy;

        /// <summary>
        /// Gets the repository factory.
        /// </summary>
        protected IRepositoryFactory RepositoryFactory { get; private set; }

        /// <summary>
        /// Dispose of the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object.
        /// </summary>
        /// <param name="disposing">Is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.CreatedEntities?.Count > 0)
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

        /// <summary>
        /// Create a test record.
        /// </summary>
        /// <param name="record">The record to create.</param>
        /// <returns>A reference to the created record.</returns>
        protected EntityReference CreateTestRecord(Entity record)
        {
            record.Id = this.CrmServiceClient.Create(record);
            var reference = record.ToEntityReference();
            this.CreatedEntities.Add(reference);

            return reference;
        }

        /// <summary>
        /// Create multiple test records.
        /// </summary>
        /// <param name="testData">The test records to create.</param>
        /// <returns>A collection of references to the created records.</returns>
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
                    ReturnResponses = true,
                },
                Requests = requestCollection,
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
                this.CreatedEntities.Add(entityReference);
                entityReferences.Add(entityReference);
            }

            return entityReferences.ToArray();
        }

        private void DeleteTestData()
        {
            var deleteRequests = new OrganizationRequestCollection();

            if (this.CreatedEntities.Count() == 0)
            {
                return;
            }

            deleteRequests.AddRange(
                this.CreatedEntities.Select(
                    e => new DeleteRequest { Target = e }));

            this.CrmServiceClient.Execute(new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                },
                Requests = deleteRequests,
            });
        }

        private string GetConnectionString(Uri developmentUrl, string username = null)
        {
            var targetEnvironment = Environment.GetEnvironmentVariable("CAKE_DYNAMICS_URL_CI");

            return $"Url={targetEnvironment ?? developmentUrl.ToString()}; Username={username ?? Environment.GetEnvironmentVariable("CAKE_DYNAMICS_USERNAME")}; Password={Environment.GetEnvironmentVariable("CAKE_DYNAMICS_PASSWORD")}; AuthType=Office365;";
        }
    }
}

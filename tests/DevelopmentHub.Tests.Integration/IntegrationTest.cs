namespace DevelopmentHub.Tests.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using DevelopmentHub.BusinessLogic;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Tooling.Connector;

    /// <summary>
    /// Base integration test class.
    /// </summary>
    public abstract class IntegrationTest : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
        /// </summary>
        public IntegrationTest()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.CreatedEntities = new List<EntityReference>();
            this.CrmServiceClient = new CrmServiceClient(this.GetConnectionString());
            this.RepositoryFactory = new RepositoryFactory(this.CrmServiceClient);
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
            if (record is null)
            {
                throw new ArgumentNullException(nameof(record));
            }

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

            if (this.CreatedEntities.Count == 0)
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

        private string GetConnectionString()
        {
            var url = Environment.GetEnvironmentVariable("DEVELOPMENTHUB_TEST_URL");
            var username = Environment.GetEnvironmentVariable("DEVELOPMENTHUB_ADMIN_USERNAME");
            var password = Environment.GetEnvironmentVariable("DEVELOPMENTHUB_ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ConfigurationException("Environment variables required for integration tests haven't set.");
            }

            //return $"Url={url}; Username={username}; Password={password}; AuthType=Office365;";
            return $"AuthType=OAuth;Username={username}; Password={password};Url={url};AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Never";
        }
    }
}

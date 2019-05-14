namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// Base repository class for late bound model
    /// Prefer Generic implementations and early bound model if possible
    /// </summary>
    public class CrmRepository : ICrmRepository
    {
        private const string EntityImageField = "entityimage";
        private OrganizationServiceContext orgSvcContext;

        public CrmRepository(IOrganizationService service, string entityName)
        {
            this.ServiceProxy = service;
            this.EntityName = entityName;
        }

        public CrmRepository(IOrganizationService service, OrganizationServiceContext context, string entityName)
        {
            this.ServiceProxy = service;
            this.EntityName = entityName;
            this.orgSvcContext = context;
        }

        public string EntityName { get; protected set; }

        protected IOrganizationService ServiceProxy { get; }

        protected OrganizationServiceContext CurrentContext
        {
            get
            {
                return this.orgSvcContext ?? (this.orgSvcContext = this.CreateNewServiceContext());
            }

            set
            {
                this.orgSvcContext = value;
            }
        }

        public virtual Entity Retrieve(Guid crmId, string[] requiredColumns)
        {
            if (requiredColumns == null || requiredColumns.Length == 0)
            {
                throw new ArgumentException($"At least one column must be specified when invoking {nameof(this.Retrieve)} method of {nameof(CrmRepository)}");
            }

            var colSet = new ColumnSet(requiredColumns.Where(c => c != null).Distinct().ToArray());
            var entity = this.ServiceProxy.Retrieve(this.EntityName, crmId, colSet);
            return entity;
        }

        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector)
            where TObject : class
        {
            var results = this.CurrentContext.CreateQuery(this.EntityName)
                                     .Where(filter)
                                     .Select(selector);

            return results;
        }

        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector, int resultStart, int numberofRowsToRetrieve)
            where TObject : class
        {
            var results = this.CurrentContext.CreateQuery(this.EntityName)
                                     .Where(filter)
                                     .Select(selector)
                                     .Skip(resultStart)
                                     .Take(numberofRowsToRetrieve);

            return results;
        }

        public virtual Guid Create(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            entity.Id = this.ServiceProxy.Create(entity);
            return entity.Id;
        }

        public virtual void Update(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            this.ServiceProxy.Update(entity);
        }

        public virtual void Delete(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            this.ServiceProxy.Delete(entity.LogicalName, entity.Id);
        }

        public virtual void BulkDelete(List<Entity> entityList, int batchSize = 100)
        {
            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                },

                Requests = new OrganizationRequestCollection()
            };

            foreach (var entity in entityList)
            {
                DeleteRequest deleteRequest = new DeleteRequest { Target = entity.ToEntityReference() };
                multipleRequest.Requests.Add(deleteRequest);
                if (multipleRequest.Requests.Count == batchSize)
                {
                    ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)this.ServiceProxy.Execute(multipleRequest);
                    multipleRequest.Requests.Clear();
                }
            }

            if (multipleRequest.Requests.Count > 0)
            {
                ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)this.ServiceProxy.Execute(multipleRequest);
            }
        }

        public void SetEntityPicture(Entity entity, byte[] picture)
        {
            this.EnsureLogicalNameIsValid(entity);

            var ent = this.Retrieve(entity.Id, new string[] { EntityImageField });
            ent[EntityImageField] = picture;

            this.Update(ent);
        }

        public void ExecuteWorkflowForEntity(Entity entity, Guid workflowId)
        {
            this.EnsureLogicalNameIsValid(entity);

            var request = new ExecuteWorkflowRequest
            {
                WorkflowId = workflowId,
                EntityId = entity.Id
            };

            this.ServiceProxy.Execute(request);
        }

        protected virtual OrganizationServiceContext CreateNewServiceContext()
        {
            return this.ServiceProxy.CreateNewCrmContext<OrganizationServiceContext>();
        }

        private void EnsureLogicalNameIsValid(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.LogicalName != this.EntityName)
            {
                throw new ArgumentException($"{nameof(entity)} must be of type {this.EntityName}");
            }
        }
    }
}
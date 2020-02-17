namespace DevelopmentHub.Repositories
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
    /// Base repository class for late-bound model.
    /// Early-bound model and repository classes should be used where possible.
    /// </summary>
    public class CrmRepository : ICrmRepository
    {
        private const string EntityImageField = "entityimage";
        private OrganizationServiceContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmRepository"/> class.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="entityLogicalName">Entity logical name.</param>
        public CrmRepository(IOrganizationService orgService, string entityLogicalName)
        {
            this.OrgService = orgService;
            this.EntityLogicalName = entityLogicalName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmRepository"/> class.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="entityLogicalName">Entity logical name.</param>
        /// <param name="context">Context.</param>
        public CrmRepository(IOrganizationService orgService, string entityLogicalName, OrganizationServiceContext context)
            : this(orgService, entityLogicalName)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets or sets entity logical name.
        /// </summary>
        public string EntityLogicalName { get; protected set; }

        /// <summary>
        /// Gets organization service.
        /// </summary>
        protected IOrganizationService OrgService { get; }

        /// <summary>
        /// Gets or sets the current context.
        /// </summary>
        protected OrganizationServiceContext CurrentContext
        {
            get
            {
                return this.context ?? (this.context = this.CreateNewServiceContext());
            }

            set
            {
                this.context = value;
            }
        }

        /// <inheritdoc/>
        public virtual Entity Retrieve(Guid entityId, string[] columns)
        {
            if (columns == null || columns.Length == 0)
            {
                throw new ArgumentException($"At least one column must be specified when invoking {nameof(this.Retrieve)} method of {nameof(CrmRepository)}");
            }

            var colSet = new ColumnSet(columns.Where(c => c != null).Distinct().ToArray());
            var entity = this.OrgService.Retrieve(this.EntityLogicalName, entityId, colSet);
            return entity;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector)
            where TObject : class
        {
            var results = this.CurrentContext.CreateQuery(this.EntityLogicalName)
                                     .Where(filter)
                                     .Select(selector);

            return results;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector, int skip, int take)
            where TObject : class
        {
            var results = this.CurrentContext.CreateQuery(this.EntityLogicalName)
                                     .Where(filter)
                                     .Select(selector)
                                     .Skip(skip)
                                     .Take(take);

            return results;
        }

        /// <inheritdoc/>
        public virtual Guid Create(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            entity.Id = this.OrgService.Create(entity);
            return entity.Id;
        }

        /// <inheritdoc/>
        public virtual void Update(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            this.OrgService.Update(entity);
        }

        /// <inheritdoc/>
        public virtual void Delete(Entity entity)
        {
            this.EnsureLogicalNameIsValid(entity);
            this.OrgService.Delete(entity.LogicalName, entity.Id);
        }

        /// <inheritdoc/>
        public virtual void BulkDelete(IEnumerable<Entity> entities, int batchSize = 100)
        {
            var multipleRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true,
                },

                Requests = new OrganizationRequestCollection(),
            };

            foreach (var entity in entities)
            {
                var deleteRequest = new DeleteRequest { Target = entity.ToEntityReference() };
                multipleRequest.Requests.Add(deleteRequest);
                if (multipleRequest.Requests.Count == batchSize)
                {
                    var multipleResponse = (ExecuteMultipleResponse)this.OrgService.Execute(multipleRequest);
                    multipleRequest.Requests.Clear();
                }
            }

            if (multipleRequest.Requests.Count > 0)
            {
                var multipleResponse = (ExecuteMultipleResponse)this.OrgService.Execute(multipleRequest);
            }
        }

        /// <inheritdoc/>
        public void SetEntityPicture(Entity entity, byte[] picture)
        {
            this.EnsureLogicalNameIsValid(entity);

            var ent = this.Retrieve(entity.Id, new string[] { EntityImageField });
            ent[EntityImageField] = picture;

            this.Update(ent);
        }

        /// <inheritdoc/>
        public void ExecuteWorkflowForEntity(Entity entity, Guid workflowId)
        {
            this.EnsureLogicalNameIsValid(entity);

            var request = new ExecuteWorkflowRequest
            {
                WorkflowId = workflowId,
                EntityId = entity.Id,
            };

            this.OrgService.Execute(request);
        }

        /// <summary>
        /// Creates a new service context.
        /// </summary>
        /// <returns>A new service context.</returns>
        protected virtual OrganizationServiceContext CreateNewServiceContext()
        {
            return this.OrgService.CreateNewCrmContext<OrganizationServiceContext>();
        }

        private void EnsureLogicalNameIsValid(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.LogicalName != this.EntityLogicalName)
            {
                throw new ArgumentException($"{nameof(entity)} must be of type {this.EntityLogicalName}");
            }
        }
    }
}
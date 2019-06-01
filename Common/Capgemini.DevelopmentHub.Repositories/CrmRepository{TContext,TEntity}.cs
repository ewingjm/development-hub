namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using Microsoft.Xrm.Sdk.Messages;

    /// <summary>
    /// Generic repository class for use with an early-bound model.
    /// </summary>
    /// <typeparam name="TContext">Organization service context.</typeparam>
    /// <typeparam name="TEntity">Early-bound entity.</typeparam>
    public class CrmRepository<TContext, TEntity> : CrmRepository, ICrmRepository<TEntity>
        where TEntity : Entity, new()
        where TContext : OrganizationServiceContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrmRepository{TContext, TEntity}"/> class.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        public CrmRepository(IOrganizationService orgService)
            : base(orgService, new TEntity().LogicalName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmRepository{TContext, TEntity}"/> class.
        /// </summary>
        /// <param name="orgService">Organization service.</param>
        /// <param name="context">Context.</param>
        public CrmRepository(IOrganizationService orgService, TContext context)
            : base(orgService, new TEntity().LogicalName, context)
        {
        }

        /// <summary>
        /// Gets the current context.
        /// </summary>
        protected new TContext CurrentContext => (TContext)base.CurrentContext;

        /// <inheritdoc/>
        public new virtual TEntity Retrieve(Guid entityId, string[] columns)
        {
            return base.Retrieve(entityId, columns).ToEntity<TEntity>();
        }

        /// <inheritdoc/>
        public virtual TObject Retrieve<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
            where TObject : class
        {
            this.EnsureParametersAreValid(selector);

            var list = this.CurrentContext.CreateQuery<TEntity>()
                                     .Where(filter)
                                     .Select(selector)
                                     .Single();
            return list;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
            where TObject : class
        {
            this.EnsureParametersAreValid(selector);

            var list = this.CurrentContext.CreateQuery<TEntity>()
                                     .Where(filter)
                                     .Select(selector);

            return list;
        }

        /// <inheritdoc/>
        public virtual IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int resultStart, int numberofRowsToRetrieve)
            where TObject : class
        {
            this.EnsureParametersAreValid(selector);

            var list = this.CurrentContext.CreateQuery<TEntity>()
                                     .Where(filter).Select(selector)
                                     .Skip(resultStart)
                                     .Take(numberofRowsToRetrieve);

            return list;
        }

        /// <inheritdoc/>
        public virtual IList<TObject> FindAll<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int pageSize = 5000, int maxRecords = 200000)
            where TObject : class
        {
            int currentRecordNo = 0;
            var pageCount = pageSize;

            if (pageSize > maxRecords)
            {
                pageCount = maxRecords;
            }

            var pageData = this.Find(filter, selector, currentRecordNo, pageCount).ToList();
            var results = new List<TObject>(pageData);

            while (pageData != null && pageData.Count > 0 && (pageData.Count == pageSize || results.Count < maxRecords))
            {
                currentRecordNo += pageData.Count;
                if (currentRecordNo + pageSize > maxRecords)
                {
                    pageCount = maxRecords - currentRecordNo;
                }

                if (pageCount > 0)
                {
                    pageData = this.Find(filter, selector, currentRecordNo, pageCount).ToList();
                    results.AddRange(pageData);
                }
                else
                {
                    pageData = null;
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public virtual Guid Create(TEntity entity)
        {
            return base.Create(entity);
        }

        /// <inheritdoc/>
        public virtual void Update(TEntity entity)
        {
            base.Update(entity);
        }

        /// <inheritdoc/>
        public virtual void Delete(TEntity entity)
        {
            base.Delete(entity);
        }

        /// <inheritdoc/>
        public virtual void BulkDelete(List<TEntity> entityList, int batchSize = 100)
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

            foreach (var entity in entityList)
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
        protected override OrganizationServiceContext CreateNewServiceContext()
        {
            var context = this.OrgService.CreateNewCrmContext<TContext>();
            return context;
        }

        private static bool FoundIdInSelector(string query)
        {
            var regex = new Regex(@"\.Id(\W){0,1}");
            var result = regex.IsMatch(query);

            return result;
        }

        private static bool InefficientQuerySelectorDetected(MemberInitExpression selectorBody)
        {
            var detected = false;
            var bindings = selectorBody?.Bindings;

            if (bindings != null)
            {
                foreach (var memberBinding in bindings.Where(x => x.BindingType == MemberBindingType.Assignment))
                {
                    var expression = ((MemberAssignment)memberBinding).Expression;

                    if (FoundIdInSelector(expression.ToString().Trim()))
                    {
                        detected = true;
                        break;
                    }
                }
            }

            return detected;
        }

        private void EnsureParametersAreValid<TObject>(Expression<Func<TEntity, TObject>> selector)
            where TObject : class
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (InefficientQuerySelectorDetected(selector.Body as MemberInitExpression))
            {
                throw new ArgumentException("It is not allowed to use generic Id field directly in the selector - use the valid entity specific id field");
            }
        }
    }
}
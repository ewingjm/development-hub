namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Xrm.Sdk;

    public interface ICrmRepository
    {
        Entity Retrieve(Guid crmId, string[] requiredColumns);

        IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector)
             where TObject : class;

        IQueryable<TObject> Find<TObject>(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, TObject>> selector, int resultStart, int numberofRowsToRetrieve)
            where TObject : class;

        Guid Create(Entity entity);

        void Update(Entity entity);

        void Delete(Entity entity);

        void BulkDelete(List<Entity> entityList, int batchSize = 100);

        void SetEntityPicture(Entity entity, byte[] picture);

        void ExecuteWorkflowForEntity(Entity entity, Guid workflowId);
    }
}
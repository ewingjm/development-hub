namespace Capgemini.DevelopmentHub.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.Xrm.Sdk;

    public interface ICrmRepository<TEntity>
        where TEntity : Entity
    {
        TEntity Retrieve(Guid id, string[] requiredColumns);

        TObject Retrieve<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
            where TObject : class;

        IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector)
            where TObject : class;

        IQueryable<TObject> Find<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int resultStart, int numberofRowsToRetrieve)
            where TObject : class;

        IList<TObject> FindAll<TObject>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TObject>> selector, int pageSize = 5000, int maxRecords = 200000)
            where TObject : class;

        Guid Create(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        void BulkDelete(List<TEntity> entityList, int batchSize = 100);
    }
}
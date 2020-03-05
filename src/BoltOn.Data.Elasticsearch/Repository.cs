using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace BoltOn.Data.Elasticsearch
{
    public class Repository<TEntity, TElasticsearchOptions> : IRepository<TEntity>
        where TEntity : BaseEntity<Guid>
        where TElasticsearchOptions : BaseElasticsearchOptions
    {
        protected virtual string IndexName { get; set; } = nameof(TEntity);

        public ElasticClient Client { get; }

        public Repository(TElasticsearchOptions elasticsearchOptions)
        {
            var connectionSettings = new ConnectionSettings(elasticsearchOptions.Uri);
            Client = new ElasticClient(connectionSettings);
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
        {
            var result = await Client.IndexAsync(entity, idx => idx.Index(IndexName), cancellationToken);
            if (!result.IsValid)
            {
                throw result.OriginalException;
            }

            return entity;
        }

        public virtual async Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, object options = null,
            CancellationToken cancellationToken = default)
        {
            var tempEntities = entities.ToList();
            foreach (var entity in tempEntities)
            {
                var result = await Client.IndexAsync(entity, idx => idx.Index(IndexName), cancellationToken);
                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }
            }
            return tempEntities;
        }

        public virtual async Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default)
        {
            var result = await Client.GetAsync<TEntity>(id.ToString(), idx => idx.Index(IndexName),
                cancellationToken);
            if (!result.IsValid)
            {
                throw result.OriginalException;
            }
            return result.Source;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default)
        {
            var result = await Client.SearchAsync<TEntity>(search =>
               search.MatchAll().Index(IndexName), cancellationToken);
            return result.Documents;
        }

        /// <summary>
        /// Elasticsearch NEST library does not support search by predicate, so use ElasticSearchModel
        /// for all the equality comparison
        /// </summary>
        public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
            object options = null, CancellationToken cancellationToken = default)
        {
            if (predicate != null)
                throw new NotImplementedException("Bolton Elasticsearch does not support search by predicate");

            if (options != null && options is ElasticsearchModel searchModel)
            {
                var dynamicQuery = new List<QueryContainer>();
                foreach (var item in searchModel.Fields)
                {
                    dynamicQuery.Add(Query<TEntity>.Match(m => m.Field(new Field(item.Key.ToLower()))
                        .Query(item.Value)));
                }

                var result = await Client.SearchAsync<TEntity>(s => s
                    .From(searchModel.From)
                    .Size(searchModel.Size)
                    .Index(IndexName)
                    .Query(q => q.Bool(b => b.Must(dynamicQuery.ToArray()))), cancellationToken);

                if (!result.IsValid)
                {
                    throw result.OriginalException;
                }

                return result.Documents;
            }
            return null;
        }

        public virtual async Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
        {
            var result = await Client.UpdateAsync(new DocumentPath<TEntity>(entity), 
                u => u.Doc(entity).Index(IndexName), cancellationToken);
            if (!result.IsValid)
            {
                throw result.OriginalException;
            }
        }

        public virtual async Task DeleteAsync(TEntity entity, object options = null,
            CancellationToken cancellationToken = default)
        {
            if (entity is BaseEntity<Guid> entityWithId)
            {
                var result = await Client.DeleteAsync<TEntity>(entityWithId.Id.ToString(),
                    idx => idx.Index(IndexName),
                    cancellationToken);
            }
        }
    }
}

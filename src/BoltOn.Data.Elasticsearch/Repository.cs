using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BoltOn.Data.Elasticsearch
{
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : class
	{
		public Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> AddAsync(IEnumerable<TEntity> entities, object options = null,
			CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}

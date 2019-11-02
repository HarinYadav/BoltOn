﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Data.EF
{
	public class Repository<TEntity, TDbContext> : IRepository<TEntity>
		where TDbContext : DbContext
		where TEntity : class
	{
		private readonly EventBag _eventBag;
		private readonly IBoltOnClock _boltOnClock;

		private TDbContext DbContext { get; set; }
		protected DbSet<TEntity> DbSets { get; private set; }

		public Repository(IDbContextFactory dbContextFactory, EventBag eventBag,
			IBoltOnClock boltOnClock)
		{
			DbContext = dbContextFactory.Get<TDbContext>();
			DbSets = DbContext.Set<TEntity>();
			_eventBag = eventBag;
			_boltOnClock = boltOnClock;
		}

		public virtual IEnumerable<TEntity> GetAll(object options = null)
		{
			return DbSets.Select(s => s).ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> GetAllAsync(object options = null, CancellationToken cancellationToken = default)
		{
			return await DbSets.ToListAsync(cancellationToken);
		}

		public virtual IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, object options = null)
		{
			var query = DbSets.Where(predicate);
			if (options is IEnumerable<Expression<Func<TEntity, object>>> includes && includes.Any())
			{
				query = includes.Aggregate(query,
				(current, include) => current.Include(include));
			}

			return query.ToList();
		}

		public virtual async Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate,
			object options = null,
			CancellationToken cancellationToken = default)
		{
			var query = DbSets.Where(predicate);
			if (options is IEnumerable<Expression<Func<TEntity, object>>> includes && includes.Any())
			{
				query = includes.Aggregate(query,
				(current, include) => current.Include(include));
			}

			return await query.ToListAsync(cancellationToken);
		}

		public virtual TEntity Add(TEntity entity, object options = null)
		{
			DbSets.Add(entity);
			SaveChanges(entity);
			return entity;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			DbSets.Add(entity);
			await SaveChangesAsync(entity, cancellationToken);
			return entity;
		}

		public virtual void Update(TEntity entity, object options = null)
		{
			DbSets.Update(entity);
			SaveChanges(entity);
		}

		public virtual async Task UpdateAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			DbSets.Update(entity);
			await SaveChangesAsync(entity, cancellationToken);
		}

		public virtual TEntity GetById(object id, object options = null)
		{
			return DbSets.Find(id);
		}

		public virtual async Task<TEntity> GetByIdAsync(object id, object options = null, CancellationToken cancellationToken = default)
		{
			return await DbSets.FindAsync(id);
		}

		public virtual async Task DeleteAsync(TEntity entity, object options = null, CancellationToken cancellationToken = default)
		{
			DbSets.Attach(entity);
			DbSets.Remove(entity);
			await SaveChangesAsync(entity, cancellationToken);
		}

		protected void SaveChanges(TEntity entity)
		{
			PublishEvents(entity);
			DbContext.SaveChanges();
		}

		protected async Task SaveChangesAsync(TEntity entity, CancellationToken cancellationToken = default)
		{
			PublishEvents(entity);
			await DbContext.SaveChangesAsync(cancellationToken);
		}

		private void PublishEvents(TEntity entity)
		{
			if (entity is BaseCqrsEntity baseCqrsEntity)
			{
				var eventsToBeProcessed = baseCqrsEntity.EventsToBeProcessed.ToList()
					.Where(w => !w.CreatedDate.HasValue);
				foreach (var @event in eventsToBeProcessed)
				{
					@event.CreatedDate = _boltOnClock.Now;
					_eventBag.EventsToBeProcessed.Add(@event);
				}

				var processedEvents = baseCqrsEntity.ProcessedEvents.ToList()
					.Where(w => !w.ProcessedDate.HasValue);
				foreach (var @event in processedEvents)
				{
					@event.ProcessedDate = _boltOnClock.Now;
				}
			}
		}
	}
}
﻿using System;
using System.Transactions;
using BoltOn.Logging;
using BoltOn.Mediator.Pipeline;
using BoltOn.Ovverides.Mediator;
using BoltOn.UoW;

namespace BoltOn.Ovverides.UoW
{
	public class UnitOfWorkOptionsBuilder : IUnitOfWorkOptionsBuilder
	{
		private readonly IBoltOnLogger<UnitOfWorkOptionsBuilder> _logger;

		public UnitOfWorkOptionsBuilder(IBoltOnLogger<UnitOfWorkOptionsBuilder> logger)
		{
			_logger = logger;
		}

		public virtual UnitOfWorkOptions Build<TResponse>(IRequest<TResponse> request) 
		{
			IsolationLevel isolationLevel;
			switch (request)
			{
				case ICommand<TResponse> _:
				case ICommand _:
					_logger.Debug("Getting isolation level for Command");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				case IQuery<TResponse> _:
					_logger.Debug("Getting isolation level for Query");
					isolationLevel = IsolationLevel.ReadCommitted;
					break;
				case IQueryUncommitted<TResponse> _:
					_logger.Debug("Getting isolation level for StaleQuery");
					isolationLevel = IsolationLevel.ReadUncommitted;
					break;
				default:
					throw new Exception("Request should implement ICommand<> or IQuery<> to enable Unit of Work.");
			}
			return new UnitOfWorkOptions { IsolationLevel = isolationLevel };
		}
	}
}
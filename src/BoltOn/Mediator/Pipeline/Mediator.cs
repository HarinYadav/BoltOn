﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Utilities;

namespace BoltOn.Mediator.Pipeline
{
	public interface IMediator
	{
		TResponse Get<TResponse>(IRequest<TResponse> request);
		Task<TResponse> GetAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));
	}

	public class Mediator : IMediator
	{
		private readonly IBoltOnLogger<Mediator> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<IInterceptor> _interceptors;

		public Mediator(IBoltOnLogger<Mediator> logger, IServiceProvider serviceProvider,
						IEnumerable<IInterceptor> interceptors)
		{
			_logger = logger;
			this._serviceProvider = serviceProvider;
			this._interceptors = interceptors;
		}

		public TResponse Get<TResponse>(IRequest<TResponse> request)
		{
			return ExecuteInterceptors(request, Handle);
		}

		public async Task<TResponse> GetAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
		{
			return await ExecuteInterceptorsAsync(request, HandleAsync, cancellationToken);
		}

		private TResponse ExecuteInterceptors<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, TResponse> handle)
		{
			_logger.Debug("Running Interceptors...");
			var next = _interceptors.Reverse().Aggregate(handle,
				   (handleDelegate, interceptor) => (req) => interceptor.Run<IRequest<TResponse>, TResponse>(req, handleDelegate));
			try 
			{
				return next.Invoke(request);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				throw;
			}
			finally
			{
				_interceptors.ToList().ForEach(m => m.Dispose());
			}
		}

		private async Task<TResponse> ExecuteInterceptorsAsync<TResponse>(IRequest<TResponse> request,
			Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> handleAsync, CancellationToken cancellationToken)
		{
			_logger.Debug("Running Interceptors...");
			var next = _interceptors.Reverse().Aggregate(handleAsync,
				   (handleDelegate, interceptor) => (req, token) => interceptor.RunAsync<IRequest<TResponse>, TResponse>(req, token, handleDelegate));
			try
			{
				return await next.Invoke(request, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				throw;
			}
			finally
			{
				_interceptors.ToList().ForEach(m => m.Dispose());
			}
		}

		private TResponse Handle<TResponse>(IRequest<TResponse> request)
		{
			var requestType = request.GetType();
			_logger.Debug($"Resolving handler for request: {requestType}");
			var genericRequestHandlerType = typeof(IRequestHandler<,>);
			var interfaceHandlerType = genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
			var handler = _serviceProvider.GetService(interfaceHandlerType);
			Check.Requires(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
			_logger.Debug($"Resolved handler: {handler.GetType()}");
			// this is to keep the request objects in the handlers strongly typed and to keep the handlers implement IRequestHandler
			// and not inherit baserequesthandler
			var decorator = (BaseRequestHandlerDecorator<TResponse>)Activator.CreateInstance(typeof(RequestHandlerDecorator<,>)
																					   .MakeGenericType(requestType, typeof(TResponse)), handler);
			var response = decorator.Handle(request);
			return response;
		}

		private async Task<TResponse> HandleAsync<TResponse>(IRequest<TResponse> request,
			CancellationToken cancellationToken)
		{
			var requestType = request.GetType();
			_logger.Debug($"Resolving handler for request: {requestType}");
			var genericRequestHandlerType = typeof(IRequestAsyncHandler<,>);
			var interfaceHandlerType = genericRequestHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));
			var handler = _serviceProvider.GetService(interfaceHandlerType);
			Check.Requires(handler != null, string.Format(Constants.ExceptionMessages.HANDLER_NOT_FOUND, requestType));
			_logger.Debug($"Resolved handler: {handler.GetType()}");
			// this is to keep the request objects in the handlers strongly typed and to keep the handlers implement IRequestAsyncHandler
			// and not inherit baserequesthandler
			var decorator = (BaseRequestAsyncHandlerDecorator<TResponse>)Activator.CreateInstance(typeof(RequestAsyncHandlerDecorator<,>)
																					   .MakeGenericType(requestType, typeof(TResponse)), handler);
			var response = await decorator.HandleAsync(request, cancellationToken);
			return response;
		}
	}
}

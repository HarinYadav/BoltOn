﻿using System.Linq;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Mediator.Middlewares;
using BoltOn.Mediator.Pipeline;
using BoltOn.UoW;

namespace BoltOn.Mediator
{
	public class MediatorRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IMediator, Pipeline.Mediator>();
			container.AddSingleton<IUnitOfWorkOptionsBuilder, UnitOfWorkOptionsBuilder>();
			RegisterMiddlewares(container);
			RegisterHandlers(context);
			RegisterAsyncHandlers(context);
		}

		private static void RegisterMiddlewares(IServiceCollection container)
		{
			container.AddTransient<IMediatorMiddleware, StopwatchMiddleware>();
			container.AddTransient<IMediatorMiddleware, UnitOfWorkMiddleware>();
		}

		private void RegisterHandlers(RegistrationTaskContext context)
		{
			var requestHandlerInterfaceType = typeof(IRequestHandler<,>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.Container.AddTransient(handler.Interface, handler.Implementation);
		}

		private void RegisterAsyncHandlers(RegistrationTaskContext context)
		{
			var requestHandlerInterfaceType = typeof(IRequestAsyncHandler<,>);
			var handlers = (from a in context.Assemblies.ToList()
							from t in a.GetTypes()
							from i in t.GetInterfaces()
							where i.IsGenericType &&
								requestHandlerInterfaceType.IsAssignableFrom(i.GetGenericTypeDefinition())
							select new { Interface = i, Implementation = t }).ToList();
			foreach (var handler in handlers)
				context.Container.AddTransient(handler.Interface, handler.Implementation);
		}
	}
}

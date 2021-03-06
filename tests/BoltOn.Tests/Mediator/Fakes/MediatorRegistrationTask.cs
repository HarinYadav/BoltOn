﻿using System;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Tests.UoW;
using BoltOn.UoW;
using BoltOn.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Mediator.Fakes
{
	public static class MediatorRegistrationTask
	{
		public static void RegisterMediatorFakes(this BoltOnOptions boltOnOptions)
		{
			var changeTrackerInterceptor = new Mock<IBoltOnLogger<ChangeTrackerInterceptor>>();
			changeTrackerInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient(s => changeTrackerInterceptor.Object);


			var boltOnClock = new Mock<IBoltOnClock>();
			var currentDateTime = DateTime.Parse("10/27/2018 12:51:59 PM");
			boltOnClock.Setup(s => s.Now).Returns(currentDateTime);
			boltOnOptions.ServiceCollection.AddTransient((s) => boltOnClock.Object);

			var testInterceptorLogger = new Mock<IBoltOnLogger<TestInterceptor>>();
			testInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => testInterceptorLogger.Object);

			var stopWatchInterceptorLogger = new Mock<IBoltOnLogger<StopwatchInterceptor>>();
			stopWatchInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
									 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => stopWatchInterceptorLogger.Object);

			var customUoWOptionsBuilder = new Mock<IBoltOnLogger<TestCustomUnitOfWorkOptionsBuilder>>();
			customUoWOptionsBuilder.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient(s => customUoWOptionsBuilder.Object);

			var uowOptionsBuilderLogger = new Mock<IBoltOnLogger<UnitOfWorkOptionsBuilder>>();
			uowOptionsBuilderLogger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			boltOnOptions.ServiceCollection.AddTransient((s) => uowOptionsBuilderLogger.Object);

			if (MediatorTestHelper.IsClearInterceptors)
				boltOnOptions.RemoveAllInterceptors();

			if (MediatorTestHelper.IsCustomizeIsolationLevel)
			{
				boltOnOptions.RemoveInterceptor<ChangeTrackerInterceptor>();
				boltOnOptions.AddInterceptor<CustomChangeTrackerInterceptor>();
				boltOnOptions.ServiceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, TestCustomUnitOfWorkOptionsBuilder>();
				var customChangeTrackerInterceptorLogger = new Mock<IBoltOnLogger<CustomChangeTrackerInterceptor>>();
				customChangeTrackerInterceptorLogger.Setup(s => s.Debug(It.IsAny<string>()))
										 .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
				boltOnOptions.ServiceCollection.AddTransient(s => customChangeTrackerInterceptorLogger.Object);
			}

			if (MediatorTestHelper.IsRemoveStopwatchInterceptor)
				boltOnOptions.RemoveInterceptor<StopwatchInterceptor>();

			boltOnOptions.AddInterceptor<TestInterceptor>();
		}
	}
}

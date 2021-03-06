using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Pluralize.NET.Core;

namespace BoltOn.Data.CosmosDb
{
    public static class Extensions
    {
        private static Pluralizer _pluralizer;

        public static string Pluralize(this string word)
        {
            _pluralizer ??= new Pluralizer();
            return _pluralizer.Pluralize(word);
        }

        public static BoltOnOptions BoltOnCosmosDbModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }

		public static IServiceCollection AddCosmosDb<TCosmosDbOptions>(this IServiceCollection serviceCollection,
			Action<BaseCosmosDbOptions> action)
			where TCosmosDbOptions : BaseCosmosDbOptions, new()
		{
			var options = new TCosmosDbOptions();
			action(options);

			serviceCollection.AddSingleton(options);

			return serviceCollection;
		}
	}
}

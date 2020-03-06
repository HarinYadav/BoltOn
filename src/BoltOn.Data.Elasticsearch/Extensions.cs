using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Pluralize.NET.Core;

namespace BoltOn.Data.Elasticsearch
{
    public static class Extensions
    {
        private static Pluralizer _pluralizer;

        public static string Pluralize(this string word)
        {
            _pluralizer ??= new Pluralizer();
            return _pluralizer.Pluralize(word);
        }

        public static BoltOnOptions BoltOnElasticsearchModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }

		public static IServiceCollection AddElasticsearch<TElasticsearchOptions>(this IServiceCollection serviceCollection,
			Action<BaseElasticsearchOptions> action)
			where TElasticsearchOptions : BaseElasticsearchOptions, new()
		{
			var options = new TElasticsearchOptions();
			action(options);
			serviceCollection.AddSingleton(options);
			return serviceCollection;
		}
	}
}

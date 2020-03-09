using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.Elasticsearch;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;


namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public static class ElasticsearchRegistrationTask
	{
		public static void RegisterElasticsearchFakes(this BoltOnOptions boltOnOptions)
		{
			if (IntegrationTestHelper.IsElasticsearchServer)
			{
				var uri = new Uri("http://localhost:9200/");

				//using var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey);
				//client.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbOptions.DatabaseName }).GetAwaiter().GetResult();

				//var documentCollection = new DocumentCollection { Id = "StudentFlattened" };
				//documentCollection.PartitionKey.Paths.Add("/studentTypeId");
				//client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(cosmosDbOptions.DatabaseName),
				//	documentCollection).GetAwaiter().GetResult();
				boltOnOptions.ServiceCollection.AddElasticsearch<TestElasticsearchOptions>(e =>
				{
					e.ConnectionSettings = new Nest.ConnectionSettings(uri);
				});

				boltOnOptions.ServiceCollection.AddTransient<IRepository<Employee>, Repository<Employee, TestElasticsearchOptions>>();
			}
		}
	}

	public class TestElasticsearchOptions : BaseElasticsearchOptions
	{
	}
}
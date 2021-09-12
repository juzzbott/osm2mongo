using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Osm2Mongo.OsmTypes;

namespace Osm2Mongo
{
    public class MongoProvider
	{

		private MongoClient _client;
		private IMongoDatabase _db;

		public MongoProvider(string mongoUrl)
		{
			Initialise(MongoUrl.Create(mongoUrl));
		}

		public MongoProvider(string dbHost, int port, string dbName)
		{
			var builder = new MongoUrlBuilder
			{
				DatabaseName = dbName,
				Server = new MongoServerAddress(dbHost, port)
			};
			var mongoUrl = builder.ToMongoUrl();
			Initialise(mongoUrl);
		}

		private void Initialise(MongoUrl mongoUrl)
		{
			_client = new MongoClient(mongoUrl);
			_db = _client.GetDatabase(mongoUrl.DatabaseName);
		}

		

		/// <summary>
		/// Prepares the collection by dropping it and recreating it.
		/// </summary>
		/// <param name="collectionName"></param>
		public async Task PrepareCollection(string collectionName)
		{
			// Drop and create the collection
			await _db.DropCollectionAsync(collectionName);
			await _db.CreateCollectionAsync(collectionName);

			IMongoCollection<OsmNode> collection = _db.GetCollection<OsmNode>(collectionName);
			
			// Create the tag keys index
			var tagsIndexModel = new CreateIndexModel<OsmNode>(new IndexKeysDefinitionBuilder<OsmNode>().Ascending(x => x.TagKeys));
			await collection.Indexes.CreateOneAsync(tagsIndexModel);

			if (collectionName.ToLower() == "nodes")
			{
				var geo2dIndexModel = new CreateIndexModel<OsmNode>(new IndexKeysDefinitionBuilder<OsmNode>().Geo2DSphere(x => x.Location));
				await collection.Indexes.CreateOneAsync(geo2dIndexModel);
			}
		}

		public async Task WriteBatch(IEnumerable<IOsmEntity> items, string collectionName)
		{
			// get the collection
			IMongoCollection<IOsmEntity> collection = _db.GetCollection<IOsmEntity>(collectionName);
			await collection.InsertManyAsync(items);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

namespace Osm2Mongo.OsmTypes
{
	public class OsmRelationMember
	{

		[BsonElement("type")]
		public string Type { get; set; }

		[BsonElement("ref")]
		public long ReferenceId { get; set; }

		[BsonElement("role")]
		public string Role { get; set; }

	}
}

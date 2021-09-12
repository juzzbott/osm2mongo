using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

namespace Osm2Mongo.OsmTypes
{
	public class OsmWay : BaseOsmEntity
	{

		[BsonElement("nodes")]
		public List<long> Nodes { get; set; }

		public OsmWay() 
			: base()
		{
			this.Nodes = new List<long>();
		}

	}
}

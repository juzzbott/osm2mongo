using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

namespace Osm2Mongo.OsmTypes
{
	public class OsmNode : BaseOsmEntity
	{

		[BsonElement("location")]
		public float[] Location { get; set; }

		public OsmNode()
			: base()
		{

		}

	}
}

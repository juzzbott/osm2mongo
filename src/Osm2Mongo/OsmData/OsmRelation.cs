using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

namespace Osm2Mongo.OsmTypes
{
	public class OsmRelation : BaseOsmEntity
	{

		[BsonElement("relations")]
		public List<OsmRelationMember> Relations { get; set; }

		public OsmRelation()
			: base()
		{
			this.Relations = new List<OsmRelationMember>();
		}

	}
}

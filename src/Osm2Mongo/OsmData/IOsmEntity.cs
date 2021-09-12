using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osm2Mongo.OsmTypes
{
	public interface IOsmEntity
	{

		long Id { get; set; }

		string User { get; set; }

		int UserId { get; set; }

		int Version { get; set; }

		int ChangeSet { get; set; }

		DateTime Timestamp { get; set; }

		List<string[]> Tags { get; set; }

		List<string> TagKeys { get; set; }
		
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using MongoDB.Bson.Serialization.Attributes;

namespace Osm2Mongo.OsmTypes
{
	public class BaseOsmEntity : IOsmEntity
	{

		[BsonId]
		public long Id { get; set; }

		[BsonElement("uname")]
		public string User { get; set; }

		[BsonElement("uid")]
		public int UserId { get; set; }

		[BsonElement("version")]
		public int Version { get; set; }

		[BsonElement("changeset")]
		public int ChangeSet { get; set; }

		[BsonElement("timestamp")]
		public DateTime Timestamp { get; set; }

		[BsonElement("tags")]
		public List<string[]> Tags { get; set; }

		[BsonElement("tagKeys")]
		public List<string> TagKeys { get; set; }

		public BaseOsmEntity()
		{
			this.Tags = new List<string[]>();
			this.TagKeys = new List<string>();
		}

		/// <summary>
		/// Creates a new instance of the IOsmEntity object
		/// </summary>
		/// <param name="dataItem"></param>
		/// <returns></returns>
		public static T Create<T>(XElement dataItem) where T : IOsmEntity
		{

			// Create the node
			T entity = Activator.CreateInstance<T>();

			// Set the ID
			entity.Id = (long)dataItem.Attribute("id");

			// Set the user details
			entity.User = (string)dataItem.Attribute("user");
			entity.UserId = (dataItem.Attribute("uid") != null ? (int)dataItem.Attribute("uid") : 0);

			// Set the change details
			entity.ChangeSet = (dataItem.Attribute("uid") != null ? (int)dataItem.Attribute("changeset") : 0);
			entity.Version = (dataItem.Attribute("uid") != null ? (int)dataItem.Attribute("version") : 0);
			entity.Timestamp = (DateTime)dataItem.Attribute("timestamp");

			XName tagName = XName.Get("tag");
			var tags = dataItem.Descendants(tagName);

			// Get all the tags
			foreach (XElement tagData in tags)
			{

				string key = (string)tagData.Attribute("k");
				string value = (string)tagData.Attribute("v");

				// Add the tag to the node
				entity.Tags.Add(new string[] { key, value });
				entity.TagKeys.Add(key);

			}

			// return the node object
			return entity;

		}

	}
}

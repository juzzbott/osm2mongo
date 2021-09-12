using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Osm2Mongo.OsmTypes;

namespace Osm2Mongo
{
    public class OsmImportManager
	{


		private string _osmFilepath { get; set; }
		private MongoProvider _mongoProvider;

		// Set some counters
		int _totalNodes = 0;
		int _totalWays = 0;
		int _totalRelations = 0;

		/// <summary>
		/// The list containing the processed osm objects.
		/// </summary>
		private List<IOsmEntity> _dataObjects = new List<IOsmEntity>();

		private const string NodesCollectionName = "nodes";
		private const string WaysCollectionName = "ways";
		private const string RelationsCollectionName = "relations";

		/// <summary>
		/// Create the timer to update the update the UI.
		/// </summary>
		Stopwatch _sw = new Stopwatch();

		#region Constructor

		/// <summary>
		/// Creates a new instance of the OsmMongoImporter object
		/// </summary>
		/// <param name="osmFile"></param>
		/// <param name="mongoHost"></param>
		/// <param name="mongoDbName"></param>
		public OsmImportManager(string osmFile, string mongoUrl)
		{

			// Set the path to the file, and the mongo database details.
			_osmFilepath = osmFile;

			// Create the mongo provider
			_mongoProvider = new MongoProvider(mongoUrl);

		}

		/// <summary>
		/// Creates a new instance of the OsmMongoImporter object
		/// </summary>
		/// <param name="osmFile"></param>
		/// <param name="mongoHost"></param>
		/// <param name="mongoDbName"></param>
		public OsmImportManager(string osmFile, string mongoHost, int mongoPort, string mongoDbName)
		{

			// Set the path to the file, and the mongo database details.
			_osmFilepath = osmFile;

			// Create the mongo provider
			_mongoProvider = new MongoProvider(mongoHost, mongoPort, mongoDbName);

		}

		#endregion

		#region Import OSM data

		/// <summary>
		/// Import the OSM data into the mongo database.
		/// </summary>
		public async Task ImportData()
		{

			// Run the pre-import functionality.
			await PreImport();

			// Get the first 100 nodes
			IEnumerable<XElement> nodeData = this.StreamOsmDataNodes().Select(i => i).Where(i => i.Name == "node");
			IEnumerable<XElement> wayData = this.StreamOsmDataNodes().Select(i => i).Where(i => i.Name == "way");
			IEnumerable<XElement> relationData = this.StreamOsmDataNodes().Select(i => i).Where(i => i.Name == "relation");

			// Loop through the nodes
			foreach (XElement dataItem in nodeData)
			{
				// Get the attribute based on name
				await ProcessDataItem(dataItem, _totalNodes, typeof(OsmNode), NodesCollectionName);
				_totalNodes++;	
			}

			// Finish of the remain nodes in the object collection.
			await BatchInsertObjects(typeof(OsmNode), NodesCollectionName);

			// Loop through the ways
			foreach (XElement dataItem in wayData)
			{
				// Get the attribute based on name
				await ProcessDataItem(dataItem, _totalWays, typeof(OsmWay), WaysCollectionName);
				_totalWays++;
			}

			// Finish of the remain nodes in the object collection.
			await BatchInsertObjects(typeof(OsmWay), WaysCollectionName);

			// Loop through the relations
			foreach (XElement dataItem in relationData)
			{
				// Get the attribute based on name
				await ProcessDataItem(dataItem, _totalRelations, typeof(OsmRelation), RelationsCollectionName);
				_totalRelations++;
			}

			// Finish of the remain nodes in the object collection.
			await BatchInsertObjects(typeof(OsmRelation), RelationsCollectionName);

			// Stop the stopwatch
			_sw.Stop();

			displayOutput();

		}

		/// <summary>
		/// Processes the data item, which adds it to the collection and batch inserts after 500 milliseconds.
		/// </summary>
		/// <param name="dataItem"></param>
		/// <param name="totalCount"></param>
		/// <param name="dataType"></param>
		/// <param name="collectionName"></param>
		private async Task ProcessDataItem(XElement dataItem, int totalCount, Type dataType, string collectionName)
		{
			// Reset the stop watch 
			if (totalCount == 0)
			{
				_sw.Restart();
				_dataObjects.Clear();
			}

			// If the timer has progressed 500 milliseconds, batch insert the current list of nodes.
			if (totalCount > 0 && _sw.ElapsedMilliseconds > 250)
			{
				await BatchInsertObjects(dataType, collectionName);

				// restart the timer
				_sw.Restart();
			}

			// Add the current node to the list for batch processing, and increment the total nodes processed.
			switch (collectionName)
			{

				case NodesCollectionName:
					_dataObjects.Add(getNodeObj(dataItem));
					break;

				case WaysCollectionName:
					_dataObjects.Add(getWayObj(dataItem));
					break;

				case RelationsCollectionName:
					_dataObjects.Add(getRelationObj(dataItem));
					break;

			}
		}

		/// <summary>
		/// Batch inserts the objects in the collection.
		/// </summary>
		/// <param name="dataType"></param>
		/// <param name="collectionName"></param>
		private async Task BatchInsertObjects(Type dataType, string collectionName)
		{
			await _mongoProvider.WriteBatch(_dataObjects, collectionName);
			_dataObjects.Clear();

			displayOutput();
		}

		#endregion

		#region Helper functions

		/// <summary>
		/// Display the progress output to the console. 
		/// </summary>
		private void displayOutput()
		{

			// Clear the current line
			Console.SetCursorPosition(0, (Console.CursorTop == 0 ? 0 : Console.CursorTop - 1));

			// Write the output to the console.
			Console.WriteLine(String.Format("Nodes processed: {0}    Ways processed: {1}    Relations processed: {2}",
				_totalNodes,
				_totalWays,
				_totalRelations));
		}

		/// <summary>
		/// This method is called prior to the data being imported, for validation and index creation purposes.
		/// </summary>
		public async Task PreImport()
		{

			// Validate the osm file exists
			if (String.IsNullOrEmpty(_osmFilepath) || !File.Exists(_osmFilepath))
			{
				throw new IOException($"The OSM data file '{_osmFilepath}' does not exist.");
			}

			// Validate the mongo server is operational. If the database does not exist, them create it. 
			await _mongoProvider.PrepareCollection(NodesCollectionName);
			await _mongoProvider.PrepareCollection(WaysCollectionName);
			await _mongoProvider.PrepareCollection(RelationsCollectionName);

			Console.WriteLine("Pre-import complete. Importing data...");
			Console.WriteLine();
			Console.WriteLine();

		}

		/// <summary>
		/// Gets all the node, way & relation elements from the node. 
		/// </summary>
		/// <returns></returns>
		private IEnumerable<XElement> StreamOsmDataNodes()
		{

			// Begin using the XML reader
			using (XmlReader reader = XmlReader.Create(_osmFilepath))
			{

				// Move the reader to the content
				reader.MoveToContent();

				// Iterate the file and get the nodes
				while (reader.Read())
				{

					switch (reader.NodeType)
					{
						// If the currently read object is an element, and it matches the node, way or relation nodes, return it. 
						case XmlNodeType.Element:
							if (reader.Name == "node" || reader.Name == "way" || reader.Name == "relation")
							{
								XElement theNode = (XElement)XElement.ReadFrom(reader);
								if (theNode != null)
								{
									yield return theNode;
								}
							}
							break;
					}
				}

			}

		}

		#endregion

		#region OSM object creators

		/// <summary>
		/// Gets a new OsmNode object from the XML object.
		/// </summary>
		/// <param name="dataItem">The XML node data.</param>
		/// <returns></returns>
		private OsmNode getNodeObj(XElement dataItem)
		{

			// Create the node
			OsmNode node = BaseOsmEntity.Create<OsmNode>(dataItem);

			// Create the float array
			float[] location = new float[2];
			location[0] = (float)dataItem.Attribute("lon");
			location[1] = (float)dataItem.Attribute("lat");
			node.Location = location;

			// return the node object
			return node;

		}

		/// <summary>
		/// Gets a new OsmWay object from the XML object.
		/// </summary>
		/// <param name="dataItem">The XML node data.</param>
		/// <returns></returns>
		private OsmWay getWayObj(XElement dataItem)
		{

			// Create the way object
			OsmWay way = BaseOsmEntity.Create<OsmWay>(dataItem);

			XName nodeName = XName.Get("nd");
			var nodes = dataItem.Descendants(nodeName);

			// Get all the tags
			foreach (XElement nodeData in nodes)
			{

				// Get the node id from the attribute
				long nodeId = (long)nodeData.Attribute("ref");

				// Add the tag to the node
				way.Nodes.Add(nodeId);

			}

			// return the way object
			return way;

		}

		/// <summary>
		/// Gets a new OsmWay object from the XML object.
		/// </summary>
		/// <param name="dataItem">The XML node data.</param>
		/// <returns></returns>
		private OsmRelation getRelationObj(XElement dataItem)
		{

			// Create the way object
			OsmRelation relation = BaseOsmEntity.Create<OsmRelation>(dataItem);

			XName memberBane = XName.Get("nd");
			var members = dataItem.Descendants(memberBane);

			// Get all the tags
			foreach (XElement memberData in members)
			{

				// Get the member data from the attributes
				string type = (string)memberData.Attribute("type");
				long refId = (long)memberData.Attribute("ref");
				string role = (memberData.Attribute("role") != null ? (string)memberData.Attribute("role") : "");

				// Add the tag to the node
				relation.Relations.Add(new OsmRelationMember()
				{
					Type = type,
					ReferenceId = refId,
					Role = role
				});

			}

			// return the way object
			return relation;

		}

		#endregion

	}
}

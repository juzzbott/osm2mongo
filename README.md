# osm2mongo
dotnet library and command line tool for importing OpenStreetMap (osm) data into a MongoDB database.

## Usage
To import the data into a MongoDB database, use the following command:
osm2mongo -i C:\path\to\data.osm -h mognodbhost:port -d dbname

The -h and -d switches are optional, and if they are omitted then localhost:27017 is the default host and osm is the default database name.

To use osm2mongo you need a raw OSM data source, and a working MongoDB server. You can download raw OSM source data from: http://download.geofabrik.de/
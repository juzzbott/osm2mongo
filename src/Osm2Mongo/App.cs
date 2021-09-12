using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Osm2Mongo
{
    public class App
    {

        private readonly ILogger<App> _logger;
        private readonly IConfiguration _config;

        public App(IConfiguration config, ILogger<App> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task Run(string[] args)
        {
            _logger.LogInformation("Starting...");

            var inputFile = getCommandParameter("-i", args);
			var mongoHost = getCommandParameter("-h", args);
			var mongoDb = getCommandParameter("-d", args);

			var connectionString = _config.GetConnectionString("DefaultConnection");
			
			// Create the importer object
			var importer = new OsmImportManager(inputFile, connectionString);

			try
			{
				await importer.ImportData();

				Console.WriteLine("Processing complete.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error importing OSM data. Message: " + ex.Message);
			}
			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.ReadLine();
			}

            _logger.LogInformation("Finished!");

            await Task.CompletedTask;
        }

        /// <summary>
		/// Gets the value of the parameter from command line arguments
		/// </summary>
		/// <param name="paramName"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private static string getCommandParameter(string paramName, string[] args)
		{

			var paramValue = "";

			// Loop through the command line parameters
			for (int i = 0; i < args.Length; i++)
			{
				// return the next parameter index, which is the value of the parameter
				if (i + 1 >= args.Length)
				{
					break;
				}
				else if (args[i] == paramName)
				{
					paramValue = args[i + 1];
					break;
				}
			}

			// return the parameter value
			return paramValue;
		}

		private static bool commandParameterExists(string paramName, string[] args)
		{

			// Loop through the command parameters, if it exists, return true, otherwise return false.
			foreach (var arg in args)
			{
				if (arg == paramName)
				{
					return true;
				}
			}

			// Command parameter not found, return false
			return false;
		}
        
    }
}
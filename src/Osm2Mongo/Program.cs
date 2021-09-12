using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Osm2Mongo
{
    class Program
    {
		public static async Task Main(string[] args)
		{
			// create service collection
			var services = new ServiceCollection();
			ConfigureServices(services);

			// create service provider
			var serviceProvider = services.BuildServiceProvider();

			// entry to run app
			await serviceProvider.GetService<App>().Run(args);
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			// configure logging
			services.AddLogging(builder =>
			{
				builder.AddConsole();
				builder.AddDebug();
			});

			// build config
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false)
				.AddEnvironmentVariables()
				.Build();

			services.AddSingleton<IConfiguration>(configuration);

			// add services:
			// services.AddTransient<IMyRespository, MyConcreteRepository>();

			// add app
			services.AddTransient<App>();
		}
    }
}

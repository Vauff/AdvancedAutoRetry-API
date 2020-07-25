using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvancedAutoRetry_API
{
	public class Program
	{
		public static string version = "1.0.2";
		public static string Token;
		public static string ZoneID;
		public static int FirewallEventMinutes;
		public static bool Debug;

		public static void Main(string[] args)
		{
			Console.WriteLine("Starting AdvancedAutoRetry-API v" + version);
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}

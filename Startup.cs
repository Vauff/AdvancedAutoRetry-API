using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AdvancedAutoRetry_API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			GraphQLHttpClient client = new GraphQLHttpClient(Configuration["GraphQLURI"], new NewtonsoftJsonSerializer());

			services.AddScoped<IGraphQLClient>(s => {
				var client = new GraphQLHttpClient(Configuration["GraphQLURI"], new NewtonsoftJsonSerializer());
				client.HttpClient.DefaultRequestHeaders.Add("X-AUTH-EMAIL", Configuration["X-AUTH-EMAIL"]);
				client.HttpClient.DefaultRequestHeaders.Add("X-AUTH-KEY", Configuration["X-AUTH-KEY"]);
				client.HttpClient.Timeout = TimeSpan.FromSeconds(10);
				return client;
			});

			services.AddScoped<FirewallEventConsumer>();
			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			Program.Token = Configuration["Token"];
			Program.ZoneID = Configuration["ZoneID"];
			Program.FirewallEventMinutes = Configuration.GetValue<int>("FirewallEventMinutes");
			Program.Debug = Configuration.GetValue<bool>("Debug");

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}

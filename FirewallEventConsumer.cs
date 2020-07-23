using AdvancedAutoRetry_API.Exceptions;
using AdvancedAutoRetry_API.Models;
using AdvancedAutoRetry_API.ResponseTypes;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedAutoRetry_API
{
	public class FirewallEventConsumer
	{
		private readonly IGraphQLClient _client;

		public FirewallEventConsumer(IGraphQLClient client)
		{
			_client = client;
		}

        public async Task<List<FirewallEvent>> GetFirewallEvents(string clientIP)
        {
            DateTime now = DateTime.UtcNow;
            int attempts = 0;
            bool commFail = false;
            bool apiError = false;

            if (Program.Debug)
                Console.WriteLine("geq: " + now.AddMinutes(Program.FirewallEventMinutes * -1).ToString("o") + " leq: " + now.AddMinutes(1).ToString("o"));

            while (attempts < 4)
            {
                try
                {
                    var query = new GraphQLRequest
                    {
                        Query = "query { viewer { zones(filter: {zoneTag: \"" + Program.ZoneID + "\"}) { firewallEventsAdaptive(filter: { datetime_geq: \"" + now.AddMinutes(Program.FirewallEventMinutes * -1).ToString("o") + "\", datetime_leq: \"" + now.AddMinutes(1).ToString("o") + "\", clientIP: \"" + clientIP + "\" }, limit: 10000, orderBy: [datetime_DESC]) { clientIP clientRequestPath }}}}"
                    };

                    var response = await _client.SendQueryAsync<ResponseData>(query);

                    if (response.Errors != null)
					{
                        if (Program.Debug)
						{
                            foreach (GraphQLError error in response.Errors)
                                Console.WriteLine("API error: " + error.Message);
						}
                        
                        apiError = true;
                        attempts++;
                        Thread.Sleep(2000);
                        continue;
					}

                    return response.Data.viewer.zones[0].firewallEventsAdaptive;
                }
                catch (Exception e) when (e is HttpRequestException || e is GraphQLHttpRequestException)
                {
                    commFail = true;
                    attempts++;
                    Thread.Sleep(2000);
                }
            }

            if (commFail && apiError)
                throw new CloudflareException("Multiple Cloudflare API errors: Errors were returned and communication failed, see debug info");
            else if (apiError)
                throw new CloudflareException("The Cloudflare API returned an error, see debug info");
            else
                throw new CloudflareException("Communication with the Cloudflare API failed");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvancedAutoRetry_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvancedAutoRetry_API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ClientDownloadedController : ControllerBase
	{
		private readonly FirewallEventConsumer _consumer;

		public ClientDownloadedController(FirewallEventConsumer consumer)
		{
			_consumer = consumer;
		}

		[HttpGet("")]
		[HttpGet("{clientIp}")]
		[HttpGet("{clientIp}/{map}")]
		public async Task<IActionResult> Get(string clientIP, string map)
		{
			try
			{
				if (clientIP == null || map == null)
					return BadRequest();

				if (!Request.Headers.ContainsKey("Token") || !Program.Token.Equals(Request.Headers["Token"]))
					return StatusCode(403);

				if (Program.Debug)
					Console.WriteLine("Handling " + clientIP + "/" + map);

				bool result = false;

				foreach (FirewallEvent firewallEvent in await _consumer.GetFirewallEvents(clientIP))
				{
					if (!firewallEvent.clientRequestPath.Contains(map + ".bsp"))
						continue;

					if (firewallEvent.clientIP.Equals(clientIP))
					{
						result = true;
						break;
					}
				}

				if (Program.Debug)
					Console.WriteLine(clientIP + "/" + map + " = " + result);

				return Ok(new ClientDownloadedResponse() { clientDownloaded = result });
			}
			catch (Exception e)
			{
				Console.WriteLine(e.GetType().ToString() + ": " + e.Message);
				Console.WriteLine(e.StackTrace);
				return StatusCode(500);
			}
		}
	}
}

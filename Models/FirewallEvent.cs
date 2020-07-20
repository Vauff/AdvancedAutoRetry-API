using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedAutoRetry_API.Models
{
	public class FirewallEvent
	{
		public string clientIP { get; set; }

		public string clientRequestPath { get; set; }
	}
}

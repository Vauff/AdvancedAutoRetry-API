using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvancedAutoRetry_API.Exceptions
{
	public class CloudflareException : Exception
	{
		public CloudflareException()
		{
		}

		public CloudflareException(string message) : base(message)
		{
		}

		public CloudflareException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}

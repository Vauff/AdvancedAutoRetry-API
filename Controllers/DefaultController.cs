using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AdvancedAutoRetry_API.Controllers
{
	[ApiController]
	[Route("{*url}", Order = 999)]
	public class DefaultController : ControllerBase
	{
		public IActionResult CatchAll()
		{
			return NotFound();
		}
	}
}

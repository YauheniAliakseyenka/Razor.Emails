using Microsoft.AspNetCore.Mvc;

namespace Razor.Emails.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class Emails : ControllerBase
	{
		[Route("preview")]
		[HttpGet]
		public IActionResult GetPreview()
		{
			return new ContentResult();
		}
	}
}

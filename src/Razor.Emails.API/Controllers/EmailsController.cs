using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Razor.Emails.Services.Interfaces;
using Razor.Emails.Templates.Emails;

namespace Razor.Emails.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EmailsController : ControllerBase
	{
		private readonly IComponentRendererService componentRenderer;

		public EmailsController(IComponentRendererService componentRenderer)
		{
			this.componentRenderer = componentRenderer;
		}

		[Route("preview")]
		[HttpGet]
		public async Task<IActionResult> GetPreview()
		{
			var htmlRaw1 = componentRenderer.RenderComponentAsync<ReturnRequested>();
			var htmlRaw2 = componentRenderer.RenderComponentAsync<ReturnRequested>();
			var htmlRaw3 = componentRenderer.RenderComponentAsync<ReturnRequested>();
			var htmlRaw4 = componentRenderer.RenderComponentAsync<ReturnRequested>();
			var htmlRaw5 = componentRenderer.RenderComponentAsync<ReturnRequested>();

			await Task.WhenAll(htmlRaw1, htmlRaw2, htmlRaw3, htmlRaw4, htmlRaw5);

			var htmlRaw = await componentRenderer.RenderComponentAsync<ReturnRequested>();

			return new ContentResult
			{
				Content = htmlRaw,
				ContentType = "text/html",
				StatusCode = 200
			};
		}
	}
}

using System.IO;
using System.Threading.Tasks;

namespace Razor.Emails.Core
{
	public readonly struct HtmlComponent
	{
		private readonly HtmlRenderer renderer;

		public HtmlComponent(HtmlRenderer renderer, int componentId, Task renderTask)
		{
			this.renderer = renderer;
			this.ComponentId = componentId;
			this.RenderTask = renderTask;
		}

		public int ComponentId { get; }

		public Task RenderTask { get; }

		public string ToHtmlString()
		{
			using var writer = new StringWriter();

			renderer.WriteComponentHtml(ComponentId, writer);

			return writer.ToString();
		}
	}
}

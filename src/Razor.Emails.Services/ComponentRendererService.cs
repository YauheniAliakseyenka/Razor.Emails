using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Razor.Emails.Core;
using Razor.Emails.Services.Interfaces;

namespace Razor.Emails.Services
{
	public class ComponentRendererService : IComponentRendererService
	{
		private readonly HtmlRenderer htmlRenderer;

		public ComponentRendererService(IServiceProvider serviceProvider)
		{
			htmlRenderer = new HtmlRenderer(serviceProvider);
		}

		public async Task<string> RenderComponentAsync<T>() where T : IComponent
		{
			return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
			{
				var content = htmlRenderer.RenderComponent<T>(ParameterView.Empty);

				await content.RenderTask;

				return content.ToHtmlString();
			});
		}

		public async Task<string> RenderComponentAsync<T>(IDictionary<string, object> parameters) where T : IComponent
		{
			ParameterView parameterView = ParameterView.FromDictionary(parameters);

			return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
			{
				var content = htmlRenderer.RenderComponent<T>(parameterView);

				await content.RenderTask;

				return content.ToHtmlString();
			});
		}
	}
}

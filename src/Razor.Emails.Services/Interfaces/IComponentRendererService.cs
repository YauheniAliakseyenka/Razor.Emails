using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Razor.Emails.Services.Interfaces
{
	public interface IComponentRendererService
	{
		Task<string> RenderComponentAsync<T>() where T : IComponent;
		Task<string> RenderComponentAsync<T>(IDictionary<string, object> parameters) where T : IComponent;
	}
}

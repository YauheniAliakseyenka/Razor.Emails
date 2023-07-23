using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Razor.Emails.Core
{
	[SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Official HtmlRenderer is supported since NET8.0 preview only")]
	public sealed partial class HtmlRenderer : Renderer
	{
		private static readonly Task CanceledRenderTask = Task.FromCanceled(new CancellationToken(canceled: true));

		public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

		public HtmlRenderer(IServiceProvider serviceProvider) : this(serviceProvider, new NullLoggerFactory())
		{
		}

		public HtmlRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory): base(serviceProvider, loggerFactory)
		{
		}

		public HtmlComponent RenderComponent<T>(ParameterView parameters)
		{
			var component = InstantiateComponent(typeof(T));
			var componentId = AssignRootComponentId(component);

			var renderTask = RenderRootComponentAsync(componentId, parameters);

			if (renderTask.IsFaulted)
			{
				ExceptionDispatchInfo.Capture(renderTask.Exception.InnerException ?? renderTask.Exception).Throw();
			}

			return new HtmlComponent(this, componentId, renderTask);
		}

		protected override void HandleException(Exception exception)
		{
			ExceptionDispatchInfo.Capture(exception).Throw();
		}

		protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
		{
			return CanceledRenderTask;
		}
	}
}

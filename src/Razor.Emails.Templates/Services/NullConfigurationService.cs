using Razor.Emails.Templates.Services.Interfaces;

namespace Razor.Emails.Templates.Services
{
	public class NullConfigurationService : IConfigurationService
	{
		public string GetAssetsDomain() => string.Empty;
	}
}

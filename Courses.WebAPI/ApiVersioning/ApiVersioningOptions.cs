using System.Text.RegularExpressions;
using Asp.Versioning;

namespace Courses.WebAPI.ApiVersioning;

public partial class ApiVersioningOptions
{
    [GeneratedRegex(@"(?<Major>\d+)(.(?<Minor>\d+))?(-(?<Status>\w+))?")]
    private static partial Regex ApiVersionTemplate();
    
    public const string ApiVersions = "ApiVersions";

    private IEnumerable<ApiVersion>? _supportedApiVersions;
    private IEnumerable<ApiVersion>? _deprecatedApiVersions;

    public IEnumerable<string> Supported { get; set; } = [];
    public IEnumerable<string> Deprecated { get; set; } = [];
    public IEnumerable<SunsetOptions> Sunset { get; set; } = [];

    public IEnumerable<ApiVersion> SupportedApiVersions => _supportedApiVersions ??= GetApiVersions(Supported);
    public IEnumerable<ApiVersion> DeprecatedApiVersions => _deprecatedApiVersions ??= GetApiVersions(Deprecated);
    public string NewestActiveApiVersion => SupportedApiVersions.Last().ToString();

    public IEnumerable<string> ProvidedApiVersions =>
    [
        .. SupportedApiVersions.Select(x => $"v{x.ToString()}"), 
        .. DeprecatedApiVersions.Select(x => $"v{x.ToString()}")
    ];

    private static IEnumerable<ApiVersion> GetApiVersions(IEnumerable<string> apiVersions)
    {
        var parsedApiVersions = new List<ApiVersion>();
        
        foreach (var apiVersion in apiVersions)
        {
            var match = ApiVersionTemplate().Match(apiVersion);

            if (!match.Success) continue;
            
            var majorVersion = int.Parse(match.Groups["Major"].Value);
            int? minorVersion = int.TryParse(match.Groups["Minor"].Value, out var minor) ? minor : default;
            var versionStatus = match.Groups["Status"].Value;
            
            parsedApiVersions.Add(new ApiVersion(majorVersion, minorVersion, versionStatus));
        }
        
        return parsedApiVersions;
    }

    public class SunsetOptions
    {
        public string ApiVersion { get; set; } = string.Empty;
        public DateOnly Effective { get; set; }
    }
}
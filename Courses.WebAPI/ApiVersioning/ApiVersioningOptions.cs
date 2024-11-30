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
    private IEnumerable<SunsetPolicy>? _sunsetPolicies;

    public IEnumerable<string> Supported { get; set; } = [];
    public IEnumerable<string> Deprecated { get; set; } = [];
    public IEnumerable<SunsetOptions> Sunset { get; set; } = [];

    public IEnumerable<ApiVersion> SupportedApiVersions => _supportedApiVersions ??= GetApiVersions(Supported);
    public IEnumerable<ApiVersion> DeprecatedApiVersions => _deprecatedApiVersions ??= GetApiVersions(Deprecated);

    public IEnumerable<SunsetPolicy> SunsetPolicies => _sunsetPolicies ??= GetSunsetPolicies();

    public string NewestActiveApiVersion => SupportedApiVersions.Last().ToString();

    public IEnumerable<string> ProvidedApiVersions =>
    [
        .. SupportedApiVersions.Select(x => $"v{x.ToString()}"),
        .. DeprecatedApiVersions.Select(x => $"v{x.ToString()}")
    ];

    private static IEnumerable<ApiVersion> GetApiVersions(IEnumerable<string> apiVersions) =>
        apiVersions.Select(GetApiVersion).ToList();

    private static ApiVersion GetApiVersion(string apiVersion)
    {
        var match = ApiVersionTemplate().Match(apiVersion);

        if (!match.Success) throw new ApplicationException($"Invalid ApiVersion {apiVersion}");

        var majorVersion = int.Parse(match.Groups["Major"].Value);
        int? minorVersion = int.TryParse(match.Groups["Minor"].Value, out var minor) ? minor : default;
        var versionStatus = match.Groups["Status"].Value;

        return new ApiVersion(majorVersion, minorVersion, versionStatus);
    }

    private IEnumerable<SunsetPolicy> GetSunsetPolicies() => Sunset.Select(sunsetOptions => new SunsetPolicy
        { ApiVersion = GetApiVersion(sunsetOptions.ApiVersion), Effective = sunsetOptions.Effective }).ToList();

    public class SunsetOptions
    {
        public string ApiVersion { get; set; } = string.Empty;
        public DateTime Effective { get; set; }
    }

    public class SunsetPolicy
    {
        public required ApiVersion ApiVersion { get; init; }
        public required DateTime Effective { get; init; }
    }
}
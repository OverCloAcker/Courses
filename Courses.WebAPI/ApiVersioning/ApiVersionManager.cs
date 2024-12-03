using Asp.Versioning;
using Asp.Versioning.Builder;

namespace Courses.WebAPI.ApiVersioning;

public class ApiVersionManager
{
    private const string BaseNeutralApiUrl = "api";
    private const string BaseVersionedApiUrl = "api/v{version:apiVersion}";

    private ApiVersionSet _apiVersionSet = default!;

    public ApiVersionManager WithApiVersionSet(ApiVersionSet apiVersionSet)
    {
        _apiVersionSet = apiVersionSet;
        return this;
    }

    public IEndpointRouteBuilder GetApi(IEndpointRouteBuilder endpoints, string urlPrefix, string groupName,
        ApiVersion apiVersion)
    {
        var routeGroup = GetApi(endpoints, urlPrefix, groupName, [apiVersion]);
        return routeGroup;
    }

    public IEndpointRouteBuilder GetApi(IEndpointRouteBuilder endpoints, string urlPrefix, string groupName,
        IEnumerable<ApiVersion> apiVersions)
    {
        var routeGroup = endpoints
            .MapGroup($"{BaseVersionedApiUrl}/{urlPrefix}")
            .WithTags(groupName)
            .WithApiVersionSet(_apiVersionSet);

        foreach (var apiVersion in apiVersions)
            routeGroup.MapToApiVersion(apiVersion);

        return routeGroup;
    }

    public static class ApiVersions
    {
        public static ApiVersion V0_1 = new(0.1);
        public static ApiVersion V1_0_Alpha = new(1.0, "alpha");
        public static ApiVersion V1_0 = new(1.0);
    }
}
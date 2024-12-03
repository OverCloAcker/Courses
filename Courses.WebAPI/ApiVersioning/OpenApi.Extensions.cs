using Asp.Versioning;
using Courses.WebAPI.Extensions;
using Scalar.AspNetCore;

namespace Courses.WebAPI.ApiVersioning;

public static partial class Extensions
{
    private static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection("OpenApi");
        var apiVersioningOptions = new ApiVersioningOptions();
        app.Configuration.GetSection(ApiVersioningOptions.ApiVersions).Bind(apiVersioningOptions);

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.MapOpenApi();

        app.MapScalarApiReference(options =>
            {
                options
                    .WithEndpointPrefix("/api/{documentName}")
                    .WithDefaultFonts(false)
                    .WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            }
        );
        app.MapGet("/api", () => Results.Redirect($"/api/v{apiVersioningOptions.NewestActiveApiVersion}"))
            .ExcludeFromDescription();

        return app;
    }

    private static IHostApplicationBuilder AddDefaultOpenApi(
        this IHostApplicationBuilder builder,
        IApiVersioningBuilder? apiVersioning = default)
    {
        var openApi = builder.Configuration.GetSection("OpenApi");
        var identitySection = builder.Configuration.GetSection("Identity");
        var apiVersioningOptions = new ApiVersioningOptions();
        builder.Configuration.GetSection(ApiVersioningOptions.ApiVersions).Bind(apiVersioningOptions);

        var scopes = identitySection.Exists()
            ? identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, string?>();


        if (!openApi.Exists())
        {
            return builder;
        }

        if (apiVersioning is not null)
        {
            // the default format will just be ApiVersion.ToString(); for example, 1.0.
            // this will format the version as "'v'major[.minor][-status]"
            var versioned = apiVersioning.AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVVV";
                options.SubstituteApiVersionInUrl = true;
            });
            var versions = apiVersioningOptions.AvailableApiVersions;
            var supportedApiVersions = apiVersioningOptions.SupportedApiVersions;
            var deprecatedApiVersions = apiVersioningOptions.DeprecatedApiVersions;
            foreach (var description in versions)
            {
                builder.Services.AddOpenApi(description, options =>
                {
                    options.ApplyApiVersionInfo(openApi.GetRequiredValue("Document:Title"),
                        openApi.GetRequiredValue("Document:Description"), supportedApiVersions, deprecatedApiVersions);
                    options.ApplyAuthorizationChecks([.. scopes.Keys]);
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyOperationDeprecatedStatus();
                    // Clear out the default servers so we can fallback to
                    // whatever ports have been allocated for the service by Aspire
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Servers = [];
                        return Task.CompletedTask;
                    });
                });
            }
        }

        return builder;
    }
}
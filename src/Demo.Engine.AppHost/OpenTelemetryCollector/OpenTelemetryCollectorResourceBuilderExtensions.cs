// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.AppHost.OpenTelemetryCollector;

public static class OpenTelemetryCollectorResourceBuilderExtensions
{
    private const string OTEL_EXPORTER_OTLP_ENDPOINT = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string DASHBOARD_OTLP_URL_VARIABLE_NAME = "ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL";
    private const string DASHBOARD_OTLP_API_KEY_VARIABLE_NAME = "AppHost:OtlpApiKey";
    private const string DASHBOARD_OTLP_URL_DEFAULT_VALUE = "http://localhost:18889";
    private const string OTEL_COLLECTOR_IMAGE_NAME = "ghcr.io/open-telemetry/opentelemetry-collector-releases/opentelemetry-collector-contrib";
    private const string OTEL_COLLECTOR_IMAGE_TAG = "0.159.0";

    public static IResourceBuilder<OpenTelemetryCollectorResource> AddOpenTelemetryCollector(this IDistributedApplicationBuilder builder, string name, string configFileLocation)
    {
        var url = builder.Configuration[DASHBOARD_OTLP_URL_VARIABLE_NAME] ?? DASHBOARD_OTLP_URL_DEFAULT_VALUE;
        var isHttpsEnabled = url.StartsWith("https", StringComparison.OrdinalIgnoreCase);

        var dashboardOtlpEndpoint = new HostUrl(url);

        var collectorResource = new OpenTelemetryCollectorResource(name);
        var resourceBuilder = builder.AddResource(collectorResource)
            .WithImage(OTEL_COLLECTOR_IMAGE_NAME, OTEL_COLLECTOR_IMAGE_TAG)
            .WithEndpoint(targetPort: 4317, name: OpenTelemetryCollectorResource.OTLP_GRPC_ENDPOINT_NAME, scheme: "http")
            .WithEndpoint(targetPort: 4318, name: OpenTelemetryCollectorResource.OTLP_HTTP_ENDPOINT_NAME, scheme: "http")
            .WithUrlForEndpoint(OpenTelemetryCollectorResource.OTLP_GRPC_ENDPOINT_NAME, u => u.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .WithUrlForEndpoint(OpenTelemetryCollectorResource.OTLP_HTTP_ENDPOINT_NAME, u => u.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .WithBindMount(configFileLocation, "/etc/otelcol-contrib/config.yaml")
            .WithEnvironment("ASPIRE_ENDPOINT", $"{dashboardOtlpEndpoint}")
            .WithEnvironment("ASPIRE_API_KEY", builder.Configuration[DASHBOARD_OTLP_API_KEY_VARIABLE_NAME])
            .WithEnvironment("ASPIRE_INSECURE", isHttpsEnabled ? "false" : "true");

        _ = builder.Eventing.Subscribe<BeforeStartEvent>((e, ct) =>
        {
            var logger = e.Services.GetRequiredService<ILogger<OpenTelemetryCollectorResource>>();
            var endpoint = collectorResource.GetEndpoint(OpenTelemetryCollectorResource.OTLP_GRPC_ENDPOINT_NAME);

            if (!endpoint.Exists)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"No {OpenTelemetryCollectorResource.OTLP_GRPC_ENDPOINT_NAME} endpoint for the collector.");
                }
                return Task.CompletedTask;
            }

            // Update all resources to forward telemetry to the collector.
            var appModel = e.Services.GetRequiredService<DistributedApplicationModel>();
            foreach (var resource in appModel.Resources)
            {
                resource.Annotations.Add(new EnvironmentCallbackAnnotation(context =>
                {
                    if (context.EnvironmentVariables.ContainsKey(OTEL_EXPORTER_OTLP_ENDPOINT))
                    {
                        if (logger.IsEnabled(LogLevel.Debug))
                        {
                            logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector.", resource.Name);
                        }

                        context.EnvironmentVariables[OTEL_EXPORTER_OTLP_ENDPOINT] = endpoint;
                    }
                }));
            }

            return Task.CompletedTask;
        });

        if (isHttpsEnabled && builder.ExecutionContext.IsRunMode && builder.Environment.IsDevelopment())
        {
            _ = resourceBuilder.WithArgs(@"--config=/etc/otelcol-contrib/config.yaml");
        }

        return resourceBuilder;
    }
}
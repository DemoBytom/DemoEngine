// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.AppHost.OpenTelemetryCollector;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var enableGrafana = builder
    .AddParameter("enable-grafana")
    .WithDescription("Enable Grafana and Prometheus for metrics visualization")
    .WithCustomInput(parameterResource => new()
    {
        InputType = InputType.Boolean,
        Name = parameterResource.Name,
        Required = true,
        Label = "Enabled",
        Placeholder = $"Enter value for {parameterResource.Name}",
        Description = parameterResource.Description,
        EnableDescriptionMarkdown = parameterResource.EnableDescriptionMarkdown,
    })
    ;

var demoEngine = builder
    .AddProject<Projects.Demo_Engine>("demo-engine", "Demo.Engine")
    .WithIconName("Games", IconVariant.Filled)
    ;

if (builder.Configuration.GetValue<bool>("Parameters:enable-grafana"))
{

    var prometheus = builder
        .AddContainer("prometheus", "prom/prometheus", "v3.14.0")
        .WithBindMount("../../prometheus", "/etc/prometheus", isReadOnly: true)
        .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
        .WithHttpEndpoint(targetPort: 9090)
        .WithUrlForEndpoint("http", u => u.DisplayText = "Prometheus Dashboard")
        .WithIconName("BookDatabase", IconVariant.Filled)
        ;

    var grafana = builder
        .AddContainer("grafana", "grafana/grafana", "13.2.0")
        .WithBindMount("../../grafana/config", "/etc/grafana", isReadOnly: true)
        .WithBindMount("../../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
        .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
        .WithHttpEndpoint(targetPort: 3000)
        .WithUrlForEndpoint("http", u => u.DisplayText = "Grafana Dashboard")
        .WithIconName("Dashboard", IconVariant.Filled)
        ;

    var otelCollector = builder
        .AddOpenTelemetryCollector("otelcollector", "../../otelcollector/config.yaml")
        .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp")
        .WithIconName("HeartPulse", IconVariant.Filled)
        ;

    demoEngine = demoEngine
        .WaitFor(grafana)
        .WaitFor(otelCollector)
        //.WithEnvironment("OTEL_RESOURCE_ATTRIBUTES", "service.instance.id=$([guid]::NewGuid())")
        ;
}

await using var app = builder
    .Build();

await app.RunAsync();

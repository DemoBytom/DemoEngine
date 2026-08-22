// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.AppHost.OpenTelemetryCollector;

public class OpenTelemetryCollectorResource(string name)
    : ContainerResource(name)
{
    internal const string OTLP_GRPC_ENDPOINT_NAME = "grpc";
    internal const string OTLP_HTTP_ENDPOINT_NAME = "http";
}
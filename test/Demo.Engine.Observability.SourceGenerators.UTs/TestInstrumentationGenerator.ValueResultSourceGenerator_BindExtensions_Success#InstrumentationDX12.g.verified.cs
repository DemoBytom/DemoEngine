//HintName: InstrumentationDX12.g.cs
namespace Demo.Engine.TestAssembly;

public partial class InstrumentationDX12
    : global::Demo.Engine.Observability.Abstractions.IInstrumentation
{
    public static string INSTRUMENTATION_SOURCE_NAME => "Demo.Engine.Platform.DirectX12";
    
    public static string VERSION
        => global::System.Reflection.CustomAttributeExtensions.GetCustomAttribute<global::System.Reflection.AssemblyInformationalVersionAttribute>(
            typeof(global::Demo.Engine.TestAssembly.InstrumentationDX12)
                .Assembly)?
        .InformationalVersion
        ?? "0.0.0";

    public static global::System.Diagnostics.Metrics.Meter Meter { get; } = new global::System.Diagnostics.Metrics.Meter(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);

    public static global::System.Diagnostics.ActivitySource ActivitySource { get; } = new global::System.Diagnostics.ActivitySource(
        name: INSTRUMENTATION_SOURCE_NAME,
        version: VERSION);
}
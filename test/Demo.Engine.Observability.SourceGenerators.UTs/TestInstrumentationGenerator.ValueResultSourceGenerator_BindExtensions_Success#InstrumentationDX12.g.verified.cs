//HintName: InstrumentationDX12.g.cs
namespace global::Demo.Engine.TestAssembly;

public partial class InstrumentationDX12
    : global::Demo.Engine.Observability.Abstractions.IInstrumentation
{
    public static string VERSION => typeof(global::Demo.Engine.TestAssembly.InstrumentationDX12)
        .Assembly
        .GetCustomAttribute<global::System.Reflection.AssemblyInformationalVersionAttribute>()?
        .InformationalVersion
        ?? "0.0.0";
}
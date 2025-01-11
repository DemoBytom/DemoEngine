// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.RootParameterType;
using static Vortice.Direct3D12.RootSignatureFlags;

namespace Demo.Engine.Platform.DirectX12;

internal static class RootSignatureHelpers
{
    public static RootParameter1 ConstantsRootParameter(
        uint numConstants,
        ShaderVisibility shaderVisibility,
        uint shaderRegister,
        uint registerSpace = 0)
        => new(
            rootConstants: new(
                shaderRegister: shaderRegister,
                registerSpace: registerSpace,
                num32BitValues: numConstants),
            visibility: shaderVisibility);

    public static RootParameter1 ConstantBufferViewRootParameter(
        ShaderVisibility shaderVisibility,
        uint shaderRegister,
        uint registerSpace = 0,
        RootDescriptorFlags flags = RootDescriptorFlags.None)
        => DescriptorRootParameter(
            parameterType: ConstantBufferView,
            shaderVisibility: shaderVisibility,
            shaderRegister: shaderRegister,
            registerSpace: registerSpace,
            flags: flags);

    public static RootParameter1 ShaderResourceViewRootParameter(
        ShaderVisibility shaderVisibility,
        uint shaderRegister,
        uint registerSpace = 0,
        RootDescriptorFlags flags = RootDescriptorFlags.None)
        => DescriptorRootParameter(
            parameterType: ShaderResourceView,
            shaderVisibility: shaderVisibility,
            shaderRegister: shaderRegister,
            registerSpace: registerSpace,
            flags: flags);

    public static RootParameter1 UnorderedAccessViewRootParameter(
        ShaderVisibility shaderVisibility,
        uint shaderRegister,
        uint registerSpace = 0,
        RootDescriptorFlags flags = RootDescriptorFlags.None)
        => DescriptorRootParameter(
            parameterType: UnorderedAccessView,
            shaderVisibility: shaderVisibility,
            shaderRegister: shaderRegister,
            registerSpace: registerSpace,
            flags: flags);

    private static RootParameter1 DescriptorRootParameter(
        RootParameterType parameterType,
        ShaderVisibility shaderVisibility,
        uint shaderRegister,
        uint registerSpace = 0,
        RootDescriptorFlags flags = RootDescriptorFlags.None)
        => new(
            parameterType: parameterType,
            rootDescriptor: new(
                shaderRegister: shaderRegister,
                registerSpace: registerSpace,
                flags: flags),
            visibility: shaderVisibility);

    public static RootSignatureFlags DenyAll = None
        | DenyVertexShaderRootAccess
        | DenyHullShaderRootAccess
        | DenyDomainShaderRootAccess
        | DenyGeometryShaderRootAccess
        | DenyPixelShaderRootAccess
        | DenyAmplificationShaderRootAccess
        | DenyMeshShaderRootAccess;
}
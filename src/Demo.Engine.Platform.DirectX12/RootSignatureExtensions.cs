// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.RootParameterType;
using static Vortice.Direct3D12.RootSignatureFlags;

namespace Demo.Engine.Platform.DirectX12;

internal static class RootSignatureExtensions
{
    extension(RootParameter1)
    {
        public static RootParameter1 ConstantsRootParameter(
            uint numConstants,
            ShaderVisibility visibility,
            uint shaderRegister,
            uint registerSpace = 0)
            => new(
                rootConstants: new(
                    shaderRegister: shaderRegister,
                    registerSpace: registerSpace,
                    num32BitValues: numConstants),
                visibility: visibility);

        public static RootParameter1 ConstantBufferViewRootParameter(
            ShaderVisibility visibility,
            uint shaderRegister,
            uint registerSpace = 0,
            RootDescriptorFlags flags = RootDescriptorFlags.None)
            => DescriptorRootParameter(
                parameterType: ConstantBufferView,
                visibility: visibility,
                shaderRegister: shaderRegister,
                registerSpace: registerSpace,
                flags: flags);

        public static RootParameter1 ShaderResourceViewRootParameter(
            ShaderVisibility visibility,
            uint shaderRegister,
            uint registerSpace = 0,
            RootDescriptorFlags flags = RootDescriptorFlags.None)
            => DescriptorRootParameter(
                parameterType: ShaderResourceView,
                visibility: visibility,
                shaderRegister: shaderRegister,
                registerSpace: registerSpace,
                flags: flags);

        public static RootParameter1 UnorderedAccessViewRootParameter(
            ShaderVisibility visibility,
            uint shaderRegister,
            uint registerSpace = 0,
            RootDescriptorFlags flags = RootDescriptorFlags.None)
            => DescriptorRootParameter(
                parameterType: UnorderedAccessView,
                visibility: visibility,
                shaderRegister: shaderRegister,
                registerSpace: registerSpace,
                flags: flags);

        public static RootParameter1 DescriptorTableRootParameter(
            ShaderVisibility visibility,
            params DescriptorRange1[] descriptorRanges)
            => new(
                 descriptorTable: new RootDescriptorTable1(descriptorRanges),
                 visibility: visibility);

        private static RootParameter1 DescriptorRootParameter(
            RootParameterType parameterType,
            ShaderVisibility visibility,
            uint shaderRegister,
            uint registerSpace = 0,
            RootDescriptorFlags flags = RootDescriptorFlags.None)
            => new(
                parameterType: parameterType,
                rootDescriptor: new(
                    shaderRegister: shaderRegister,
                    registerSpace: registerSpace,
                    flags: flags),
                visibility: visibility);
    }

    public static RootSignatureFlags DenyAll = None
        | DenyVertexShaderRootAccess
        | DenyHullShaderRootAccess
        | DenyDomainShaderRootAccess
        | DenyGeometryShaderRootAccess
        | DenyPixelShaderRootAccess
        | DenyAmplificationShaderRootAccess
        | DenyMeshShaderRootAccess;
}
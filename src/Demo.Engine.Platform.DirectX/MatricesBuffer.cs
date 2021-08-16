using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Demo.Engine.Platform.DirectX
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct MatricesBuffer
    {
        public MatricesBuffer(
            Matrix4x4 modelToWorldMatrix,
            Matrix4x4 viewProjectionMatrix) =>
            (ModelToWorldMatrix, ViewProjectionMatrix) = (modelToWorldMatrix, viewProjectionMatrix);

        public Matrix4x4 ModelToWorldMatrix { get; }
        public Matrix4x4 ViewProjectionMatrix { get; }

        public static readonly int SizeInBytes = Unsafe.SizeOf<MatricesBuffer>();
    }
}
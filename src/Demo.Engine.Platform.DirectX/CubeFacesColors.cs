using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX
{
    public readonly struct CubeFacesColors
    {
        public CubeFacesColors(Color4 face1, Color4 face2, Color4 face3, Color4 face4, Color4 face5, Color4 face6)
        {
            Face1 = face1;
            Face2 = face2;
            Face3 = face3;
            Face4 = face4;
            Face5 = face5;
            Face6 = face6;
        }

        public Color4 Face1 { get; }
        public Color4 Face2 { get; }
        public Color4 Face3 { get; }
        public Color4 Face4 { get; }
        public Color4 Face5 { get; }
        public Color4 Face6 { get; }

        public static int SizeInBytes = Marshal.SizeOf<CubeFacesColors>();
    }
}
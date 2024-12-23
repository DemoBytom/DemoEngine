cbuffer colorsBuffer : register(b0)
{
    float4 f1;
    float4 f2;
    float4 f3;
    float4 f4;
    float4 f5;
    float4 f6;
}

float4 main(float4 color: color, uint tid: SV_PrimitiveID) : SV_Target
{
    [branch]
    switch (tid / 2)
    {
        case 0:
            return f1;
        case 1:
            return f2;
        case 2:
            return f3;
        case 3:
            return f4;
        case 4:
            return f5;
        case 5:
            return f6;
        default:
            return color;
    }
}
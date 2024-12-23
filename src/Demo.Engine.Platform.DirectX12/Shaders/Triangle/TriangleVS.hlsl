struct VSOut
{
    float4 col : color;
    float4 pos : SV_Position;
};

cbuffer constatnBuffer : register(b0)
{
    matrix transform;
    matrix worldViewProjection;
};


VSOut main(float3 pos : position, float4 color : color)
{
    VSOut vsout;
    vsout.pos = float4(pos, 1.0f);
    vsout.pos = mul(vsout.pos, transform);
    vsout.pos = mul(vsout.pos, worldViewProjection);
    vsout.col = color;
    return vsout;
}
struct VSOut
{
    float4 col : color;
    float4 pos : SV_Position;
};

cbuffer constatnBuffer
{
    matrix transform;
};

VSOut main(float2 pos : position, float4 color : color)
{
    VSOut vsout;
    vsout.pos = mul(float4(pos.x, pos.y, 0.0f, 1.0f), transform);
    vsout.col = color;
    return vsout;
}
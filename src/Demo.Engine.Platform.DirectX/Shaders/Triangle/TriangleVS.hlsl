struct VSOut
{
    float4 col : color;
    float4 pos : SV_Position;
};

VSOut main(float2 pos : position, float4 color : color)
{
    VSOut vsout;
    vsout.pos = float4(pos.x, pos.y, 0.0f, 1.0f);
    vsout.col = color;
    return vsout;
}
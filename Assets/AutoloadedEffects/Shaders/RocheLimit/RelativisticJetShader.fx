sampler noiseTexture : register(s1);

float globalTime;
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(float4(input.Position.xyz, 1), uWorldViewProjection);
    output.Position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    float glow = tex2D(noiseTexture, coords * float2(1, 1) + float2(globalTime * -3, 0));
    glow = tex2D(noiseTexture, coords + float2(globalTime * -3 + glow * 0.2, 0));
    
    float horizontalDistanceFromCenter = distance(coords.y, 0.5);
    float innerGlow = smoothstep(0.4, 0.02, horizontalDistanceFromCenter);
    
    return input.Color * saturate(glow) + innerGlow * input.Color.a + smoothstep(0.7, 1, coords.x) / horizontalDistanceFromCenter * 0.07;
}

technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

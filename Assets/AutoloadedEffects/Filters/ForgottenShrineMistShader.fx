sampler baseTexture : register(s0);
sampler noiseTexture : register(s1);
sampler liquidTexture : register(s2);

float globalTime;
float2 zoom;
float2 screenPosition;
float2 oldScreenPosition;
float2 targetSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 screenOffset = (screenPosition - oldScreenPosition) / targetSize;
    float2 liquidTextureCoords = (coords - 0.5) / zoom + 0.5 + screenOffset;
    
    return tex2D(baseTexture, coords);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity : register(C0);
float uSaturation;
float uCircularRotation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 overallImageSize;
matrix uWorldViewProjection;
float4 uShaderSpecificData;

float4 PixelFunction(float2 coords : TEXCOORD0) : COLOR0
{
    return 1;
}
technique Technique1
{
    pass ShieldPass
    {
        PixelShader = compile ps_2_0 PixelFunction();
    }
}
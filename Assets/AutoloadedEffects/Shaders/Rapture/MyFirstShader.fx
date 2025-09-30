sampler BaseTexture : register(s0);
sampler NoiseTexture : register(s1);

float4 Color; // flame tint
float Time;
float DistortionStrength = 0.05;
float ScrollSpeed = 0.4;

float StretchX = 1; // wider features
float StretchY = 0.6; // taller features

float FlareStart = 0.3; // Y (0 = bottom, 1 = top) where flare begins
float MaxFlareStrength = 0.1; // max sideways push at top

float4 CoreColor; // color near center line
float4 EdgeColor; // color near edges

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
   
    // Scroll noise upward
    // Animate & stretch noise
    float2 noiseUV = uv;
    noiseUV.y += Time * ScrollSpeed;
    noiseUV.x *= StretchX;
    noiseUV.y *= StretchY;
    float noise = tex2D(NoiseTexture, noiseUV).r;

    // Compute flare factor (0 before FlareStart, 1 at top)
    float flareFactor = saturate((uv.y - FlareStart) / (1 - FlareStart));

    // Scale distortion: base + flare
    float totalDistortion = DistortionStrength - flareFactor * MaxFlareStrength;

    // Distorted UVs
    float2 distortedUV = uv + float2((noise - 0.5) * totalDistortion, 0);

    // Sample overlay sprite
    float4 flameCol = tex2D(BaseTexture, distortedUV);

    // Distance from center line (0 at center, 1 at edges)
    float distFromCenter = abs(uv.x - 0.5) * 2.0;

    // Blend colors: Core → Edge
    float4 finalColor = lerp(CoreColor, EdgeColor, distFromCenter);
    
    return flameCol * finalColor;
}


technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
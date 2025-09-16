sampler baseTexture : register(s0);
sampler psychedelicTexture : register(s1);
sampler noiseTexture : register(s2);

float globalTime;
float opacity;
float intensityFactor;
float psychedelicExponent;
float colorAccentuationFactor;
float3 colorToAccentuate;
float4 goldColor;
float4 psychedelicColorTint;


float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 originalCoords = coords;
    
    float intensity = intensityFactor;
    
        // Apply a general microdistortion to everything.
    float microdistortionTime = globalTime * 0.7;
    float warpNoise = tex2D(noiseTexture, coords * 1.3 + globalTime * 0.1) * 0.05;
    float microdistortionX = tex2D(noiseTexture, coords * 0.8 + microdistortionTime * float2(0.1, 0.1) + warpNoise);
    float microdistortionY = tex2D(noiseTexture, coords * 0.9 + microdistortionTime * float2(0.05, -0.1) - warpNoise);
    float2 microdistortion = float2(microdistortionX, microdistortionY) * intensity * 0.01;
    //coords += microdistortion;
    
        // Distort the edges of the screen.
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5), distance(coords, 0.5));
    float edgeDistortion = sin(polar.x * 2 + polar.y * 50 + globalTime * 0.6) * smoothstep(0.4, 0.5, polar.y) * 0.03;
    float2 edgeDistortedPolar = polar + float2(edgeDistortion * intensity, 0);
    float2 edgeDistortedCoords = 0.5 + float2(cos(edgeDistortedPolar.x), sin(edgeDistortedPolar.x)) * edgeDistortedPolar.y;
    
        // Calculate a psychedelic color based on polar coordinates.
    edgeDistortedPolar.x = edgeDistortedPolar.x / 6.283 + 0.5 - globalTime * 0.01;
    float edgePsychedelicInfluence = pow(smoothstep(0.1, 0.6, polar.y), psychedelicExponent) * intensity;
    float2 psychedelicCoords = (edgeDistortedPolar + float2(0, globalTime * -0.2)) * float2(2, 0.1);
    
    float edgeMask = smoothstep(0.45, 0.6, polar.y);
    float4 edgeColor = (tex2D(psychedelicTexture, psychedelicCoords) + psychedelicColorTint) * edgeMask;
    
        // Calculate a twinkle value.
        //float2 twinkleCoords = round(originalCoords * 1000) / 1000;
        //float4 twinkleColor = CalculateStarBrightness(twinkleCoords, 0.95) * pow(edgePsychedelicInfluence, 4);
    
    float4 color = tex2D(baseTexture, edgeDistortedCoords);
    
        // Accentuate a target color.
    float3 distanceFromTargetColor = abs(color.rgb - colorToAccentuate);
        // Clamp to [0,1] so it can’t go negative
    float closenessToTargetColor = saturate(1 - dot(distanceFromTargetColor, 1));
    float targetColorAccentuation = pow(closenessToTargetColor, 3.5) * colorAccentuationFactor;
    color *= 1 + targetColorAccentuation;
    
    float2 blurOffset = 0.001;
    float4 glow = (tex2D(baseTexture, coords + float2(0, 1) * blurOffset) + tex2D(baseTexture, coords + float2(0, -1) * blurOffset) +
                       tex2D(baseTexture, coords + float2(1, 0) * blurOffset) + tex2D(baseTexture, coords + float2(-1, 0) * blurOffset)) * intensity * 0.2;
    
    float4 baseResult = color * opacity + edgeColor + glow;
    float fadeToGold = lerp(0.1, 0.35, baseResult.r); //+ microdistortionX * 0.1;
     

    return lerp(baseResult, goldColor , intensity * fadeToGold);
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
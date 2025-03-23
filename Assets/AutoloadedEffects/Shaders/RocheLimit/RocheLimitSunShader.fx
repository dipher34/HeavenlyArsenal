sampler fireNoiseTexture : register(s0);
sampler accentNoiseTexture : register(s1);
sampler uvOffsetNoiseTexture : register(s2);

float globalTime;
float coronaIntensityFactor;
float sphereSpinTime;
float3 mainColor;
float3 darkerColor;
float3 coronaColor;
float3 subtractiveAccentFactor;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Calculate the distance to the center of the sun. This is magnified a bit for intensity purposes in later equations.
    float2 coordsNormalizedToCenter = coords * 2 - 1;
    float distanceFromCenterSqr = dot(coordsNormalizedToCenter, coordsNormalizedToCenter) * 2;
    float starOpacity = smoothstep(0.5, 0.32, distanceFromCenterSqr);
    
    // Calculate coordinates relative to the sphere.
    // This pinch factor effectively ensures that the UVs are relative to a circle, rather than a rectangle.
    // This helps SIGNIFICANTLY for making the texturing look realistic, as it will appear to be traveling on a
    // sphere rather than on a sheet that happens to overlay a circle.
    float spherePinchFactor = (1 - sqrt(abs(1 - distanceFromCenterSqr))) / distanceFromCenterSqr + 0.045;
    float2 sphereCoords = coords * spherePinchFactor * 1.5 + float2(sphereSpinTime, 0);
    
    // Calculate the star brightness texture from the sphere coordinates.
    float starCoordsOffset = tex2D(fireNoiseTexture, sphereCoords).r * 0.41 + globalTime * 0.3;
    float2 starCoords = sphereCoords + float2(starCoordsOffset, 0);
    float3 starBrightnessTexture = tex2D(fireNoiseTexture, starCoords);
    
    // Calculate the glow interpolant. The closer a pixel is to the center, the stronger this value is.
    float starGlow = saturate(1 - distanceFromCenterSqr * 0.91);
    
    // Combine various aforementioned values into the base result:
    // 1. The result is brighter the higher the pinch factor is. This makes colors at the cross direction edges a little bit weaker.
    // 2. The result is brighter the higher the star glow is.
    // 3. The result is brightened relative to the brightness texture. This gives variance in the brightness of the result, and keeps it crisp.
    float3 result = spherePinchFactor * mainColor * 0.777 + starGlow * darkerColor + starBrightnessTexture;
    
    // Apply subtractive texturing to the result, skewing things towards a darker orange red based on accent noise and the inverse of the brightness.
    // This allows the result to have darker patches on it.
    float2 uvOffset = tex2D(uvOffsetNoiseTexture, sphereCoords + float2(globalTime * -0.4, 0));
    result = lerp(result, darkerColor, saturate(1 - starBrightnessTexture.r) * 0.8);
    result -= (1 - subtractiveAccentFactor) * tex2D(accentNoiseTexture, sphereCoords * 2 - uvOffset.yx * 0.012).r * 1.1;
    
    // Apply sharp brightness texturing to the result, as though lava is flowing through it. The textures this creates are thin but very bright, like lava rivers.
    result += pow(tex2D(accentNoiseTexture, sphereCoords * 1.2 + uvOffset * 0.04).r, 2) * 2.1;
    
    float2 polar = float2(atan2(coords.y - 0.5, coords.x - 0.5) / 6.283 + 0.5, distance(coords, 0.5));
    float coronaRadiusEdge = 0.4 + tex2D(accentNoiseTexture, polar * float2(2, 1) + float2(0, -0.4) * globalTime) * 0.1;
    
    float distanceFromEdge = distance(distanceFromCenterSqr, coronaRadiusEdge);
    float coronaBrightness = saturate(0.1 / distanceFromEdge) * smoothstep(0.4, 0, distanceFromEdge);
    
    // Combine everything together.
    return (starOpacity * float4(result, 1) + float4(coronaColor, 1) * coronaBrightness) * sampleColor;
}

technique Technique1
{
    pass AutoloadPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
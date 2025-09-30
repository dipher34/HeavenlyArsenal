sampler uImage0 : register(s0);

//screams
float uTime;
float4 uColor;
matrix uWorldViewProjection;

float3 StartColor;
float3 EndColor;

struct VertexShaderInput
{
    float4 position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.position, uWorldViewProjection);
    output.position = pos;
    
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    const float pi = 3.14159265359;

    // Across the strip (width) and along the strip (length)
    float stripWidth = coords.y;
    float alongStrip = coords.x - uTime * 0.5;

    // Base texture sample (scrolled to animate)
    float2 texCoords = float2(stripWidth, frac(alongStrip));
    float4 baseTex = tex2D(uImage0, texCoords);

    // Noise for distortion and flicker
    float noiseVal = tex2D(uImage0, texCoords * 2.0).r;
    texCoords.x += (noiseVal - 0.5) * 0.08 * stripWidth;
    float4 fireTex = tex2D(uImage0, texCoords);

   
    float xFade = smoothstep(0, 0.1, coords.x) * (1 - coords.x); // fade near tip
    float edgeLine = coords.y > 1 - ((1 - sin((coords.x) * 2)) * 0.1 + 0.06) * pow(xFade, 1.02);
    
    float4 edgeHighlight = edgeLine * float4(1, 1, 1, 1);

  
    float3 fireColor = lerp(StartColor, EndColor,   pow(stripWidth, 1.5));
    float4 coreColor = fireTex * float4(fireColor, 1.0);
    
    // Decay/noise factor
    float decay = pow(abs(noiseVal) * 3 * xFade * coords.y, coords.x * 4);

    // Tip fadeout (smooth at end of trail)
    float tipFade = 1.0 - smoothstep(0.8, 1.0, coords.x);

    // Final color = core + edge highlight
    float4 col = coreColor * (1 + decay * 2 * sqrt(coords.y)) + edgeHighlight;
   
   
    return col * tipFade * xFade;;
}


technique Technique1
{
    pass AutoloadPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

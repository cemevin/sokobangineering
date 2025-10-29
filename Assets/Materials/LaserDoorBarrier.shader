Shader "Custom/LaserDoorBarrier"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0, 1, 1, 1)
        _EmissionColor ("Emission Color", Color) = (0, 3, 3, 1)
        _Alpha ("Alpha", Range(0, 1)) = 0.7
        
        [Header(Animation)]
        _GridScrollSpeedX ("Grid Scroll Speed X", Float) = 0.0
        _GridScrollSpeedY ("Grid Scroll Speed Y", Float) = 1.0
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 0.5
        
        [Header(Grid Pattern)]
        _GridScale ("Grid Scale", Float) = 10.0
        _GridThickness ("Grid Thickness", Range(0.01, 0.5)) = 0.1
        _GridColor ("Grid Color", Color) = (2, 2, 2, 1)
        
        [Header(Edge Glow)]
        _EdgeGlow ("Edge Glow", Range(0, 10)) = 5.0
        _EdgePower ("Edge Power", Range(0.1, 10)) = 2.0
        
        [Header(Overall Glow)]
        _GlowIntensity ("Overall Glow Intensity", Range(1, 5)) = 2.0
        _InnerGlow ("Inner Glow", Range(0, 3)) = 1.0
        
        [Header(Noise Effects)]
        _NoiseScale1 ("Noise Scale 1", Float) = 8.0
        _NoiseSpeed1 ("Noise Speed 1", Float) = 0.3
        _NoiseIntensity1 ("Noise Intensity 1", Range(0, 1)) = 0.4
        
        _NoiseScale2 ("Noise Scale 2", Float) = 15.0
        _NoiseSpeed2 ("Noise Speed 2", Float) = 0.8
        _NoiseIntensity2 ("Noise Intensity 2", Range(0, 1)) = 0.2
        
        [Header(Flicker Effect)]
        _FlickerSpeed ("Flicker Speed", Float) = 12.0
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.3)) = 0.1
        
        [Header(Energy Distortion)]
        _DistortionScale ("Distortion Scale", Float) = 5.0
        _DistortionSpeed ("Distortion Speed", Float) = 0.5
        _DistortionIntensity ("Distortion Intensity", Range(0, 0.1)) = 0.03
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _EmissionColor;
                float _Alpha;
                float _GridScrollSpeedX;
                float _GridScrollSpeedY;
                float _PulseSpeed;
                float _PulseIntensity;
                float _GridScale;
                float _GridThickness;
                float4 _GridColor;
                float _EdgeGlow;
                float _EdgePower;
                float _GlowIntensity;
                float _InnerGlow;
                float _NoiseScale1;
                float _NoiseSpeed1;
                float _NoiseIntensity1;
                float _NoiseScale2;
                float _NoiseSpeed2;
                float _NoiseIntensity2;
                float _FlickerSpeed;
                float _FlickerIntensity;
                float _DistortionScale;
                float _DistortionSpeed;
                float _DistortionIntensity;
            CBUFFER_END
            
            // Improved hash function
            float hash(float2 p)
            {
                p = frac(p * float2(443.8975, 397.2973));
                p += dot(p.xy, p.yx + 19.19);
                return frac(p.x * p.y);
            }
            
            // Smooth noise with better interpolation
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0); // Smoother interpolation
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Fractal noise for more complex patterns
            float fractalNoise(float2 p, int octaves)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < octaves; i++)
                {
                    value += noise(p * frequency) * amplitude;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;
                
                // Sample main texture (static)
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // Multiple noise layers for complex energy patterns
                float2 noiseUV1 = uv * _NoiseScale1 + time * _NoiseSpeed1;
                float2 noiseUV2 = uv * _NoiseScale2 + time * _NoiseSpeed2 * 1.3; // Different speed multiplier
                
                float noise1 = fractalNoise(noiseUV1, 3);
                float noise2 = fractalNoise(noiseUV2, 2);
                
                // Combine noises
                float combinedNoise = lerp(noise1, noise2, 0.5);
                combinedNoise = lerp(1.0, combinedNoise, _NoiseIntensity1);
                
                // Secondary noise for variation
                float detailNoise = noise(noiseUV2 * 2.0) * _NoiseIntensity2;
                combinedNoise += detailNoise;
                
                // Flicker effect (like electrical interference)
                float flicker = sin(time * _FlickerSpeed + noise1 * 10.0) * 0.5 + 0.5;
                flicker = lerp(1.0 - _FlickerIntensity, 1.0, flicker);
                
                // Energy distortion for grid
                float2 distortionUV = uv * _DistortionScale + time * _DistortionSpeed;
                float distortion = fractalNoise(distortionUV, 2) * 2.0 - 1.0;
                float2 distortedUV = uv + distortion * _DistortionIntensity;
                
                // Create SCROLLING grid pattern with noise influence
                float2 gridUV = distortedUV * _GridScale;
                gridUV += float2(_GridScrollSpeedX, _GridScrollSpeedY) * time;
                gridUV += noise1 * 0.1; // Add noise to grid position
                
                float2 gridFrac = frac(gridUV);
                float2 gridLines = abs(gridFrac - 0.5) * 2.0;
                
                // Create grid with noise influence
                float gridX = step(1.0 - _GridThickness, gridLines.x);
                float gridY = step(1.0 - _GridThickness, gridLines.y);
                float grid = max(gridX, gridY);
                
                // Add noise to grid intensity
                grid *= combinedNoise;
                
                // Enhanced pulsing effect
                float pulse = sin(time * _PulseSpeed + noise1 * 2.0) * 0.5 + 0.5;
                pulse = lerp(1.0 - _PulseIntensity, 1.0, pulse);
                pulse *= flicker; // Combine with flicker
                
                // Enhanced Fresnel effect
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                fresnel = pow(fresnel, _EdgePower);
                
                // Inner glow effect (center is brighter)
                float2 centerUV = abs(uv - 0.5) * 2.0;
                float centerDistance = length(centerUV);
                float innerGlow = 1.0 - saturate(centerDistance);
                innerGlow = pow(innerGlow, 2.0) * _InnerGlow;
                
                // Combine all color effects
                half3 baseColor = _Color.rgb * mainTex.rgb * combinedNoise;
                half3 gridEffect = _GridColor.rgb * grid * pulse * _GlowIntensity;
                half3 emission = _EmissionColor.rgb * pulse * combinedNoise * _GlowIntensity;
                half3 edgeGlow = _EmissionColor.rgb * fresnel * _EdgeGlow * flicker;
                half3 innerGlowEffect = _EmissionColor.rgb * innerGlow * pulse;
                
                // Final color combination with enhanced glow
                half3 finalColor = (baseColor + gridEffect + emission + edgeGlow + innerGlowEffect) * _GlowIntensity;
                
                // Enhanced alpha calculation with noise
                half alpha = _Alpha * mainTex.a * combinedNoise;
                alpha += grid * 0.4 * pulse; // Grid areas more opaque
                alpha += fresnel * 0.5; // Edge glow adds alpha
                alpha += innerGlow * 0.3; // Inner glow adds alpha
                alpha *= flicker; // Flicker affects alpha
                alpha = saturate(alpha);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
Shader "Hidden/WayOut2/FogComposite"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass // Composite: _BlitTexture + _VisionMask
        {
            Name "Composite"
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Blit.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            TEXTURE2D_X(_VisionMask);
            SAMPLER(sampler_VisionMask);

            float _Darkness;
            float _Desaturate;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings  { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionHCS = GetFullScreenTriangleVertexPosition(i.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(i.vertexID);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half3 src  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv).rgb;
                half  mask = SAMPLE_TEXTURE2D_X(_VisionMask,   sampler_VisionMask,   i.uv).r;

                half  lum   = dot(src, half3(0.2126, 0.7152, 0.0722));
                half3 desat = lerp(src, lum.xxx, _Desaturate);
                half3 dark  = desat * (1.0h - _Darkness);

                half3 col = lerp(dark, src, mask); // mask=1 -> src, mask=0 -> dark
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass // Copy tmp -> src
        {
            Name "Copy"
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCopy

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fullscreen.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Blit.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings  { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionHCS = GetFullScreenTriangleVertexPosition(i.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(i.vertexID);
                return o;
            }

            half4 FragCopy(Varyings i) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.uv);
            }
            ENDHLSL
        }
    }
}
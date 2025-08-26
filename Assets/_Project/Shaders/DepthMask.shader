Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags{ "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags{ "RenderType"="Opaque" }
            ZWrite On
            ZTest LEqual
            ColorMask RGBA

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionHCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float depth01 = i.positionHCS.z / i.positionHCS.w;
                depth01 = depth01 * 0.5 + 0.5;
                return half4(depth01, depth01, depth01, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Tags{ "RenderType"="Transparent" }
            ZWrite On
            ZTest LEqual
            ColorMask RGBA

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings  { float4 positionHCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(posWS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float depth01 = i.positionHCS.z / i.positionHCS.w;
                depth01 = depth01 * 0.5 + 0.5;
                return half4(depth01, depth01, depth01, 1);
            }
            ENDHLSL
        }
    }
}
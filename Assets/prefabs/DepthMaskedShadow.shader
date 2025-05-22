
Shader "Shader Graphs/DepthMaskedShadow"
{
    Properties
    {
        _Color("Color", Color) = (1, 0, 0, 1)
        _EdgeFalloff("Edge Falloff", Float) = 4.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Name "DepthMask"
            ZWrite On
            ColorMask 0
        }
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _EdgeFalloff;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv * 2.0 - 1.0; // Centered UVs
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float dist = length(IN.uv);
                float alpha = pow(saturate(1.0 - dist), _EdgeFalloff);
                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}

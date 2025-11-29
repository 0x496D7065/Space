Shader "Custom/Pixelate"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _PixelCount("Pixel Resolution", Float) = 200
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            ZWrite Off Cull Off ZTest Always

            Pass
            {
                HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings {
                    float4 positionHCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                float _PixelCount;

                Varyings Vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                    OUT.uv = IN.uv;
                    return OUT;
                }

                half4 Frag(Varyings IN) : SV_Target
                {
                    float2 uv = IN.uv;

                    // Pixelisation
                    uv = floor(uv * _PixelCount) / _PixelCount;

                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                }

                ENDHLSL
            }
        }
}
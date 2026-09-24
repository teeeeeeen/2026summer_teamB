Shader "Custom/ScrollingRowShader"
{
    Properties
    {
        _BaseMap ("Background Texture", 2D) = "white" {}

        _OffsetX ("Offset X", Float) = 0
        _OffsetY ("Offset Y", Float) = 0

        _RowOffset ("Row Offset", Range(0, 1)) = 0.5

        _Rows ("Rows", Float) = 4
        _TileX ("Horizontal Repeat", Float) = 2
        _TileY ("Vertical Repeat", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Forward"

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _OffsetX;
            float _OffsetY;

            float _RowOffset;
            float _Rows;
            float _TileX;
            float _TileY;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // C#から受け取ったスクロール量
                uv.x += _OffsetX;
                uv.y += _OffsetY;

                // 繰り返し用のUV
                float2 tiledUV;

                tiledUV.x = uv.x * _TileX;
                tiledUV.y = uv.y * _TileY;

                // 何行目かを判定
                float row = floor(tiledUV.y);

                // 1行おきに横へずらす
                if (fmod(row, 2.0) >= 1.0)
                {
                    tiledUV.x += _RowOffset;
                }

                // 画像を繰り返す
                tiledUV.x = frac(tiledUV.x);
                tiledUV.y = frac(tiledUV.y);

                return SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    tiledUV
                );
            }

            ENDHLSL
        }
    }
}
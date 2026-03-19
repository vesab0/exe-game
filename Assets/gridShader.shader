Shader "Custom/Grid"
{
    Properties
    {
        _BgColor ("Background Color", Color) = (0.05, 0.12, 0.12, 1)
        _LineColor ("Line Color", Color) = (0.10, 0.22, 0.22, 1)
        _GridSize ("Grid Size", Float) = 20.0
        _LineWidth ("Line Width", Float) = 0.02
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            float4 _BgColor, _LineColor;
            float _GridSize, _LineWidth;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Scale UV by grid density
                float2 grid = frac(i.uv * _GridSize);

                // How close are we to a grid line?
                float2 lines = step(grid, _LineWidth) + step(1.0 - _LineWidth, grid);
                float onLine = saturate(lines.x + lines.y);

                return lerp(_BgColor, _LineColor, onLine);
            }
            ENDCG
        }
    }
}
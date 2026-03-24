Shader "Custom/CRTEffect"
{
    Properties
    {
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.25
        _ScanlineCount ("Scanline Count", Float) = 200
        _GreenTint ("Green Tint", Range(0, 1)) = 0.3
        _Vignette ("Vignette Strength", Range(0, 2)) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            float _ScanlineStrength;
            float _ScanlineCount;
            float _GreenTint;
            float _Vignette;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Scanlines — alternating dark lines
                float scanline = step(0.5, frac(i.uv.y * _ScanlineCount));
                float scanDark = 1.0 - scanline * _ScanlineStrength;

                // Vignette
                float2 vig = i.uv * (1.0 - i.uv.yx);
                float vigValue = 1.0 - pow(1.0 - vig.x * vig.y * 15.0, _Vignette);

                // Green phosphor tint
                float3 tint = float3(_GreenTint * 0.1, _GreenTint * 0.3, _GreenTint * 0.05);

                float alpha = (1.0 - scanDark) * 0.6 + vigValue * 0.5;
                float3 color = tint + vigValue * 0.3;

                return fixed4(color, alpha);
            }
            ENDCG
        }
    }
}
Shader "Hidden/PixelateEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle(PIXELATE)] _Pixelate ("Pixelate", Float) = 0
        _PixelateTex ("Pixelate Texture", 2D) = "black" {}
        _PixelAmount ("Pixel Amount", Range(1, 1000)) = 200
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _PixelateTex;
            float _PixelAmount;

            fixed4 frag (v2f i) : SV_Target
            {
                float pixelateValue = tex2D(_PixelateTex, i.uv).r;
                pixelateValue = (1 - pixelateValue) * (1 - pixelateValue) * _PixelAmount;
                i.uv = round(i.uv * pixelateValue) / pixelateValue;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}

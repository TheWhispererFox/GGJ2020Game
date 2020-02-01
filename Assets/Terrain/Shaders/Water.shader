Shader "Custom/Water"
{
	Properties
	{
		mainTexture("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

		sampler2D mainTexture;
		float3 colors[1];
		float colorStrength;
		float textureScale;

        struct Input
        {
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float3 scaledWorldPos = IN.worldPos / textureScale;

			/*float3 xProjection = tex2D(mainTexture, scaledWorldPos.yz);
			float3 yProjection = tex2D(mainTexture, scaledWorldPos.xz);
			float3 zProjection = tex2D(mainTexture, scaledWorldPos.xy);*/

			float3 textureColor = tex2D(mainTexture, scaledWorldPos.xz) * (1 - colorStrength);
			float3 baseColor = colors[0] * colorStrength;

			o.Albedo = baseColor + textureColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

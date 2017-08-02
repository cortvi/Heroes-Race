Shader "Custom/Waterfall_Surface"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Albedo ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _Bump ("Normal map", 2D) = "bump" {}
		[NoScaleOffset] _Gloss ("Specular", 2D) = "grey" {}
		[NoScaleOffset] _Disp ("Displacement", 2D) = "grey" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent" }

		CGPROGRAM
		#pragma surface surf Water
		#pragma noforwardadd

		uniform fixed4 _Color;
		uniform sampler2D _Albedo;
		uniform sampler2D _Bump;

		// STRUCTS
		struct Input
		{
			float2 uv_Albedo;
		};

		struct Surface
		{
			fixed3 Albedo;  // diffuse color
			float3 Normal;  // tangent space normal, if written
			fixed3 Emission;
			half Specular;  // specular power in 0..1 range
			fixed Gloss;    // specular intensity
			fixed Alpha;    // alpha for transparencies

		};
		// END STRUCTS

		fixed4 LightingWater ( Surface s, float3 lightDir, float3 viewDir, fixed atten )
		{
			float4 col = float4 (s.Albedo, 1);
			col.rgb *= dot(s.Normal, viewDir) >= 0.75 ? 1 : 0;

			return col;
		}

		void surf ( Input i, inout Surface s )
		{
			s.Albedo = tex2D(_Albedo, i.uv_Albedo) * _Color;
			s.Normal = UnpackNormal( tex2D (_Bump, i.uv_Albedo) );
		}
		ENDCG
	}
	FallBack "Diffuse"
}

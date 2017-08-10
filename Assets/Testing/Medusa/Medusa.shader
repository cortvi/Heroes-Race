Shader "Custom/Medusa_masked_variant"
{
	Properties 
	{
		_Mask ("Mask texture", 2D) = "white" {}

		_Color ("Color", Color) = (1,1,1,1)

		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[Normal]
		_Bump ("Normal", 2D) = "bump" {}

		_FColor ("Fresnel Color", Color)		 = (1,1,1,1)
		_FPower ("Fresnel power", Range(0, 10)) = 5.0
		_R0		("Fresnel R0",	  Range(0, .1))  = 0.05
//		_Glossiness ("Smoothness", Range(0,1)) = 0.5
//		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		// Pass[0]
		//=> Simple pass
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows keepalpha
		#pragma target 3.0

		#define UTEX(name) tex2D(name, i.uv##name)

		struct Input
		{
//			float2 uv_MainTex;
			float2 uv_Mask;
			float2 uv_Bump;
		};

		uniform sampler2D _Mask;
		uniform fixed4 _Color;
//		uniform sampler2D _MainTex;
//		uniform half _Glossiness;
//		uniform half _Metallic;

		void surf (Input i, inout SurfaceOutputStandard o)
		{
//			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = _Color.rgb;
//			o.Metallic = _Metallic;
//			o.Smoothness = _Glossiness;
			o.Alpha = UTEX(_Mask).a;
		}
		ENDCG

		Blend OneMinusDstAlpha DstAlpha
		CGPROGRAM
		#pragma surface surf Rim fullforwardshadows keepalpha
		#pragma target 3.0
		#define UTEX(name) tex2D(name, i.uv##name)

		struct Input
		{
//			float2 uv_MainTex;
			float2 uv_Mask;
			float2 uv_Bump;
		};
		struct Surface 
		{
			fixed3 Albedo;
			float3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			float Alpha;
		};

		uniform sampler2D _Bump;
		uniform float4 _FColor;
		uniform float _FPower;
		uniform float _R0;

		void surf ( Input i, inout Surface s )
		{
			i.uv_Bump += _Time.x;
			s.Normal = UnpackNormal(UTEX(_Bump));
		}

		fixed4 LightingRim ( Surface s, float3 lightDir, float3 viewDir, half atten )
		{
			float fresnel;
			fresnel = saturate ( 1.0 - dot(s.Normal, viewDir) );
			fresnel = pow (fresnel, _FPower);
			fresnel = _R0 + (1. - _R0) * fresnel;

			float3 col = lerp(_FColor.rgb*0.2, _FColor.rgb, fresnel);

			return fixed4 (col, fresnel);
		}
		ENDCG
	}

	FallBack "Diffuse"
}

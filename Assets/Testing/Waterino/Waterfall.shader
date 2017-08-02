// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/Waterfall"
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
		
		CGINCLUDE
		#include "UnityCG.cginc"

		fixed4 _Color;
		fixed4 _Albedo_ST;
		sampler2D _Albedo;
		sampler2D _Bump;
		struct fData
		{
			float4 px : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 normal : COLOR0;
			float3 worldPos : COLOR1;
			float3 worldNormal : COLOR2;
		};

		fData vert ( float3 vert : POSITION, float2 uv : TEXCOORD0, float3 normal : NORMAL )
		{
			fData f;
			f.px = UnityObjectToClipPos(vert);
			f.uv = TRANSFORM_TEX(uv,_Albedo);
			f.normal = normal;
			f.worldPos = normalize( mul( unity_ObjectToWorld, float4(vert,1) ));
			f.worldNormal = UnityObjectToWorldNormal (normal);

			return f;
		}

		fixed4 frag ( fData f ) : SV_TARGET
		{
			float3 nTex = UnpackNormal(tex2D(_Bump, f.uv));
			float3 normal = normalize(f.normal + nTex);

			float3 viewDir = normalize ( mul( _World2Object, normalize ( _WorldSpaceCameraPos - f.worldPos ).xyz ) );

			float4 col = tex2D(_Albedo, f.uv);
			col *= dot (normal, viewDir);

			return col;
		}
		ENDCG

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	FallBack "Diffuse"
}

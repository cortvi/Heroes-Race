// Per pixel bumped refraction.
// Uses a normal map to distort the image behind, and
// an additional texture to tint the color.

Shader "FX/Glass/Stained BumpDistort" {
Properties {
	_BumpAmt  ("Distortion", range (0,128)) = 10
	_MainTex ("Tint Color (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

Category {

	// We must be transparent, so other objects are drawn before this one.
	Tags { "Queue"="Transparent" "RenderType"="Opaque" }


	SubShader {

		// This pass grabs the screen behind the object into a texture.
		// We can access the result in the next pass as _GrabTexture
		GrabPass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
		}
		
		// Main pass: Take the texture grabbed above and use the bumpmap to perturb it
		// on to the screen
		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord: TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float4 uvgrab : TEXCOORD0;
	float2 uvbump : TEXCOORD1;
	float2 uvmain : TEXCOORD2;
	UNITY_FOG_COORDS(3)
};

float _BumpAmt;
float4 _BumpMap_ST;
float4 _MainTex_ST;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uvgrab = ComputeGrabScreenPos(o.vertex);
	o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );
	o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );
	UNITY_TRANSFER_FOG(o,o.vertex);
	return o;
}

sampler2D _GrabTexture;
float4 _GrabTexture_TexelSize;
sampler2D _BumpMap;
sampler2D _MainTex;

half4 frag (v2f i) : SV_Target
{
	#if UNITY_SINGLE_PASS_STEREO
	i.uvgrab.xy = TransformStereoScreenSpaceTex(i.uvgrab.xy, i.uvgrab.w);
	#endif

	// calculate perturbed coordinates
	half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg; // we could optimize this by just reading the x & y without reconstructing the Z
	float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
	#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
		i.uvgrab.xy = offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvgrab.z) + i.uvgrab.xy;
	#else
		i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
	#endif

	half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
	half4 tint = tex2D(_MainTex, i.uvmain);
	col *= tint;
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}
ENDCG
		}
	}

	// ------------------------------------------------------------------
	// Fallback for older cards and Unity non-Pro

	SubShader {
		Blend DstColor Zero
		Pass {
			Name "BASE"
			SetTexture [_MainTex] {	combine texture }
		}
	}
}

}

/*
 * => OLD
		CGINCLUDE
		uniform fixed4 _FColor;
		uniform float _FPower;
		uniform float _R0;
		
		uniform fixed4 _Dark;
		uniform fixed4 _Light;

//		uniform sampler2D _Albedo;
		uniform sampler2D _Waves;
		uniform sampler2D _Detail;
		uniform float2 _Speed;

		// STRUCTS
		struct Input   
		{
			float2 uv_Albedo;
			float2 uv_Waves;
			float2 uv_Detail;
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
		ENDCG

		//--------

		CGPROGRAM
		#pragma surface surf Lambert alpha

//		fixed4 LightingWater ( Surface s, float3 lightDir, float3 viewDir, fixed atten )
//		{
//			float4 col = float4 (s.Albedo, s.Alpha);
//
//			float fresnel;
//			fresnel = saturate ( 1.0 - dot(s.Normal, viewDir) );
//			fresnel = pow (fresnel, _FPower);
//			fresnel = _R0 + (1. - _R0) * fresnel;
//
//			return col * fresnel * _FColor;
//		}

		void surf ( Input i, inout SurfaceOutput s )
		{
			float2 uv = i.uv_Albedo.xy + _Time.x * _Speed;
			s.Albedo = tex2D(_Albedo, uv ).rgb * _Color.rgb;
			s.Normal = UnpackNormal( tex2D (_Bump, uv ) );
		}
		ENDCG

		Blend One Zero

		CGPROGRAM
		#pragma surface surf Water alpha

		fixed4 LightingWater ( Surface s, float3 lightDir, float3 viewDir, fixed atten )
		{
			float4 col = float4 (s.Albedo, s.Alpha);

			float fresnel;
			fresnel = saturate ( 1.0 - dot(s.Normal, viewDir) );
			fresnel = pow (fresnel, _FPower);
			fresnel = _R0 + (1. - _R0) * fresnel;

			return float4(_FColor.rgb, fresnel);
		}

		void surf ( Input i, inout Surface s )
		{
			float2 uv = i.uv_Albedo.xy + _Time.x * _Speed;
			s.Albedo = tex2D(_Albedo, uv ).rgb * _Color.rgb;
			s.Normal = UnpackNormal( tex2D (_Bump, uv ) );
			s.Alpha = (tex2D (_Gloss, uv ) * _Color.a).r;
		}
		ENDCG
*/

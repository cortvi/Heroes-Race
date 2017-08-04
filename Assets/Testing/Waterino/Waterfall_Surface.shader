// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Waterfall_Surface"
{
	Properties 
	{
		_FColor ("Fresnel Color",	Color)		   = (1,1,1,1)
		_FPower ("Fresnel power",	Range(0, 10) ) = 5.0
		_R0		("Fresnel R0",		Range(0, 1) )  = 0.05

		_Dark	("Water dark color",  Color) = (0,0,0,0)
		_Light	("Water light color", Color) = (1,1,1,1)

		_Albedo		("Albedo (RGB)",		  2D)	  = "white" {}
		_Speed		("Water Speed (XY + ZW)", Vector) = (0, 0, 0, 0)
		_Waves		("Waves (normal map)",	  2D)	  = "bump" {}
		_WavesMul	("Waves strenght",		  float)  = 1.0
		_Detail		("Details (normal map)",  2D)	  = "bump" {}
		_DetailMul	("Detail strenght",		  float)  = 1.0

		_BumpAmt  ("Distortion", range (0,128)) = 10
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }

		// This pass grabs the screen behind the object into a texture.
        // We can access the result in the next pass as _Screen
        GrabPass 
		{
            Tags { "LightMode" = "Always" }
        }

        // Main pass: Take the texture grabbed above and use the bumpmap to perturb it
        // on to the screen
        Pass 
		{
            Tags { "LightMode" = "Always" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex		: SV_POSITION;
				float2 uv_albedo	: TEXCOORD0;
				float2 uv_waves		: TEXCOORD1;
				float2 uv_detail	: TEXCOORD2;
				float4 uv_grab		: TEXCOORD3;
			};

			uniform sampler2D _Albedo;
			uniform float4 _Albedo_ST;
			uniform sampler2D _Waves;
			uniform float4 _Waves_ST;
			uniform float _WavesMul;
			uniform sampler2D _Detail;
			uniform float4 _Detail_ST;
			uniform float _DetailMul;

			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			float4 _Speed;
			float _BumpAmt;

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv_albedo = TRANSFORM_TEX( v.texcoord, _Albedo );
				o.uv_waves = TRANSFORM_TEX( v.texcoord, _Waves );
				o.uv_detail = TRANSFORM_TEX ( v.texcoord, _Detail );
				o.uv_grab = ComputeGrabScreenPos (o.vertex);

				return o;
			}

			half4 frag (v2f i) : SV_Target 
			{
				float3 waves = UnpackNormal(tex2D( _Waves, i.uv_waves+_Time.x*_Speed.xy )) * _WavesMul;
				float3 detail = UnpackNormal(tex2D( _Detail, i.uv_detail+_Time.x*_Speed.zw )) * _DetailMul;

				float2 offset = waves * _BumpAmt * _GrabTexture_TexelSize.xy;
				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
					i.uv_grab.xy = offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uv_grab.z) + i.uv_grab.xy;
				#else
					i.uv_grab.xy = offset * i.uv_grab.z + i.uv_grab.xy;
				#endif

				float4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uv_grab));
				fixed4 tint = tex2D(_Albedo, i.uv_albedo);
				col *= tint;

				return col;
			}
			ENDCG
        }
	}
	FallBack "Diffuse"
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

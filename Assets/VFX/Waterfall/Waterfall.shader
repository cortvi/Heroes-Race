Shader "Custom/Waterfall"
{
	Properties 
	{
		[Header (Color and disortion)]
		_Dark	 ("Dark water color",  Color)			= (0,0,0,1)
		_Lit	 ("Lit water color",   Color)			= (1,1,1,1)
		_ColPow  ("Color Balnace",	   Range(0.0, 1.5))	= 0.2
		_BumpAmt ("Distortion Amount", range (0,128))	= 10
		_Speed	 ("Water Speed",	   Vector)			= (0, 0, 0, 0)
		// (X,Y) are for Waves texture
		// (Z,W) are fore Detail texture

		[Header (Fresnel)]
		_R0		("Fresnel R0",	  Range(0.0, 0.1)) = 0.05
		_FPower ("Fresnel power", Range(0.0, 10.0)) = 5.0
		
		[Header (Textures)]
		[Normal]
		_Waves		("Waves (normal map)", 2D)	  = "bump" {}
		_WavesMul	("Waves strenght",	   float) = 1.0
		_WavesFres	("Waves fresnel",	   float) = 1.0
		
		[Normal]
		_Detail		("Noise (normal map)", 2D)	  = "bump" {}
		_DetailMul	("Noise strenght",	   float) = 1.0
	}
	SubShader  
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Cull Back

		CGINCLUDE
		// Some utilities
		#define SPEED(st) _Time.x * _Speed.st
		#define UTEX(name) tex2D(name, i.uv##name)
		ENDCG

		// This pass grabs the screen behind the object into a texture.
        // We can access the result in the next pass as "_Background"
        GrabPass 
		{
            Tags { "LightMode" = "Always" }
			"_Background"
        }

		// Color pass: Compute base color and fresnel
		CGPROGRAM
		// avoid fog and keep alpha value to avoid dissortion airfacts
		// (aka. dissorting objects in front of the water)
		#pragma surface surf Water keepalpha nofog
		#pragma target 3.0
		struct Input 
		{
			float2 uv_Waves;
			float2 uv_Detail;
		};
		
		uniform float4 _Dark;
		uniform float4 _Lit;
		uniform float4 _Speed;

		uniform float _ColPow;
		uniform float _FPower;
		uniform float _R0;

		uniform sampler2D _Waves;
		uniform float _WavesFres;
		uniform sampler2D _WavesMask;

		uniform sampler2D _Detail;
		uniform float _DetailFres;
		uniform sampler2D _DetailMask;

		void surf (Input i, inout SurfaceOutput s ) 
		{
			// Calculate normal bump
			i.uv_Waves += SPEED(xy);
			float3 waves = UnpackNormal(UTEX(_Waves)) * _WavesFres;
			s.Normal = waves;
		}

		half4 LightingWater ( SurfaceOutput s, float3 lightDir, float3 viewDir, fixed atten )
		{
			// Calculate fresnel amount
			float fresnel;
			fresnel = saturate ( 1.0 - dot(s.Normal, viewDir) );
			fresnel = pow (fresnel, _FPower);
			fresnel = _R0 + (1. - _R0) * fresnel;
			// Lerp color and iluminate fresnel
			float4 col = lerp(_Dark, _Lit, _ColPow);
			col +=  _Lit * fresnel;
			// To avoid dissortion artifacts, I paint black the alpha channel
			// so I can recognize them later
			// (since other scene objects won't have their alpha channel black)
			// This is NOT a difinitive solution though.
			col.a = 0;

			return col;
		}
		ENDCG

		// We grab the screen again to dissort the base color as well
		// This is not very optimized, best solution would be to read the first
		// grab pass in the surface shader and multiply it by the fragment color
		GrabPass 
		{
			Tags { "LightMode" = "Always" }
			"_Surface"
		}

        // Bump pass: Take the texture grabbed above and use the bumpmap to perturb it
        Pass 
		{
            Tags { "LightMode" = "Always" }
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f 
			{
				float4 vertex		: SV_POSITION;
				float2 uv_waves		: TEXCOORD1;
				float2 uv_noise		: TEXCOORD2;
				float4 uv_grab		: TEXCOORD3;
			};

			sampler2D _Surface;
			sampler2D _Background;
			float4 _Background_TexelSize;

			uniform sampler2D _Waves;
			uniform float4 _Waves_ST;
			uniform float _WavesMul;

			uniform sampler2D _Detail;
			uniform float4 _Detail_ST;
			uniform float _DetailMul;

			float4 _Speed;
			float _BumpAmt;

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv_waves = TRANSFORM_TEX( v.texcoord, _Waves);
				o.uv_noise = TRANSFORM_TEX (v.texcoord, _Detail);
				o.uv_grab = ComputeGrabScreenPos (o.vertex);
				return o;
			}

			half4 frag (v2f i) : SV_Target 
			{
				// Calculate bumped surface
				i.uv_waves += SPEED(xy);
				i.uv_noise += SPEED(zw);
				float3 waves = UnpackNormal(tex2D( _Waves, i.uv_waves) ) * _WavesMul;
				float3 noise = UnpackNormal(tex2D( _Detail, i.uv_noise) ) * _DetailMul;
				float3 bump = waves + noise;
				float2 offset = bump * _BumpAmt * _Background_TexelSize.xy;

				float4 dissort = i.uv_grab;
				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
				dissort.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uv_grab.z);
				#else
				dissort.xy += offset * i.uv_grab.z;
				#endif

				// Correct grabbed dissortion
				fixed4 surfOld = tex2Dproj (_Surface, UNITY_PROJ_COORD(i.uv_grab));
				fixed4 surfNew = tex2Dproj (_Surface, UNITY_PROJ_COORD(dissort));		// Alpha channel is black if dissorting water surface
				fixed4 surf = lerp ( surfNew, surfOld, surfNew.a);
				fixed4 bgOld = tex2Dproj( _Background, UNITY_PROJ_COORD(i.uv_grab));
				fixed4 bgNew = tex2Dproj( _Background, UNITY_PROJ_COORD(dissort));
				fixed4 bg = lerp ( bgNew, bgOld, surfNew.a);
				
				// Mix background and water surface
				return saturate (min (surf, bg));
			}
			ENDCG
        }
	}
	FallBack "Diffuse"
}
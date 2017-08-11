Shader "Custom/Waterfall"
{
	Properties 
	{
		_Dark	("Dark water color", Color)  = (0,0,0,1)
		_Lit	("Lit water color",	 Color)	 = (1,1,1,1)
		_Speed	("Water Speed",		 Vector) = (0, 0, 0, 0)

		_BumpAmt  ("Distortion Amount", range (0,128)) = 10

		_FColor ("Fresnel Color", Color)		 = (1,1,1,1)
		_FPower ("Fresnel power", Range(0, 10)) = 5.0
		_R0		("Fresnel R0",	  Range(0, .1))  = 0.05

		_Waves		("Waves (normal map)", 2D)	  = "bump" {}
		_WavesMul	("Waves strenght",	   float) = 1.0
		_WavesFres	("Waves fresnel",	   float) = 1.0
		[NoScaleOffset]
		_WavesMask	("Waves color mask",   2D)	  = "white" {}

		_Noise		("Noise (normal map)", 2D)	  = "bump" {}
		_NoiseMul	("Noise strenght",	   float) = 1.0
		[NoScaleOffset]
		_NoiseMask	("Noise color mask",   2D)	  = "white" {}

	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }

		CGINCLUDE
		#define FADE _SinTime.z * 0.5 +0.5
		#define SPEED(st) _Time.x * _Speed.st
		#define UTEX(name) tex2D(name, i.uv##name)
		ENDCG

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

			sampler2D _CameraDepthTexture;
			sampler2D _GrabTexture;
			float4 _GrabTexture_TexelSize;

			uniform fixed4 _Dark;

			uniform sampler2D _Waves;
			uniform float4 _Waves_ST;
			uniform float _WavesMul;

			uniform sampler2D _Noise;
			uniform float4 _Noise_ST;
			uniform float _NoiseMul;

			float4 _Speed;
			float _BumpAmt;

			v2f vert (appdata_base v) 
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv_waves = TRANSFORM_TEX( v.texcoord, _Waves);
				o.uv_noise = TRANSFORM_TEX (v.texcoord, _Noise);
				o.uv_grab = ComputeGrabScreenPos (o.vertex);
				return o;
			}

			half4 frag (v2f i) : SV_Target 
			{
				i.uv_waves += SPEED(xy);
				i.uv_noise += SPEED(zw);
				float3 waves = UnpackNormal(tex2D( _Waves, i.uv_waves) ) * _WavesMul;
				float3 noise = UnpackNormal(tex2D( _Noise, i.uv_noise) ) * _NoiseMul;
				float3 bump = waves + noise;
				float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;

				float4 dissort = i.uv_grab;
				#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
					dissort.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uv_grab.z);
				#else
					dissort.xy += offset * i.uv_grab.z;
				#endif

				//*1
				float4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(dissort));

				/// This shit doesn't work => fix z-bad dissortion
//				float4 fix = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uv_grab));

//				float reff = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(dissort)));
//				if (LinearEyeDepth(reff) < dissort.z)
//					col = fix;

				return col*_Dark;
			}
			ENDCG
        }
		
		// Color pass: Mix dissorted background
		// with water color based on fresnel
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma surface surf Water keepalpha nofog
		#pragma target 3.0

		struct Input   
		{
			float2 uv_Waves;
			float2 uv_WavesMask;
			float2 uv_Noise;
			float2 uv_NoiseMask;
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
		
		uniform float4 _Dark;
		uniform float4 _Lit;
		uniform float4 _Speed;

		uniform float4 _FColor;
		uniform float _FPower;
		uniform float _R0;

		uniform sampler2D _Waves;
		uniform float _WavesFres;
		uniform sampler2D _WavesMask;
		uniform sampler2D _NoiseMask;

		void surf (Input i, inout Surface s ) 
		{
			// Calculate normal bump
			i.uv_Waves += SPEED(xy);
			float3 waves = UnpackNormal(UTEX(_Waves)) * _WavesFres;
			s.Normal = waves;

			i.uv_WavesMask = i.uv_Waves;
			s.Alpha = UTEX(_WavesMask).r;

			// Calculate noise mask
//			i.uv_NoiseMask += SPEED(zw);
//			s.Alpha = UTEX(_NoiseMask).r * FADE;
		}

		half4 LightingWater ( Surface s, float3 lightDir, float3 viewDir, fixed atten )
		{
			float fresnel;
			fresnel = saturate ( 1.0 - dot(s.Normal, viewDir) );
			fresnel = pow (fresnel, _FPower);
			fresnel = _R0 + (1. - _R0) * fresnel;

			float4 col = lerp(_Dark, _Lit, pow(fresnel, s.Alpha));
			col.a = fresnel;

			return col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
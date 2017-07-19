// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Waterfall"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Bump ("Normal Map", 2D) = "bump" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent-1" }

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog   // make fog work
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 n : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 vD : TEXCOORD1;
				float4 col : COLOR0;
				UNITY_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			sampler2D _Bump;
			float4 _Bump_ST;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.col = mul(unity_ObjectToWorld, v.n);
				o.vD = normalize(WorldSpaceViewDir(v.vertex));
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Bump);
				UNITY_TRANSFER_FOG(o,o.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 n = normalize(tex2D ( _Bump, i.uv ).xyz);
				fixed4 col = dot ( i.col+n, i.vD );
				
				UNITY_APPLY_FOG(i.fogCoord, i.col);
				return col;
			}
			ENDCG
		}
	}
}

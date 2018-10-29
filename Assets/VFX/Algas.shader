Shader "Custom/Algas"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Cutoff ("Base Alpha cutoff", Range (0.0,0.9)) = 0.5
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Root ("Root height", Float) = 0.0
	}

	SubShader 
	{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }

		Cull Off
		CGPROGRAM
		#pragma surface surf Standard vertex:vert alphatest:_Cutoff addshadow
		#pragma target 3.0
		struct Input
		{
			float2 uv_MainTex;
		};
		fixed4 _Color;
		sampler2D _MainTex;
		float _Root;

		void vert (inout appdata_full v) 
		{
			float amount = sin (v.vertex.z + _Time.y + v.vertex.x);
			amount *= step ( _Root, v.vertex.z ) * 0.1;

			v.vertex += v.tangent * amount;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Standard"
}

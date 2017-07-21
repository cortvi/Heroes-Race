Shader "Custom/Waterfall"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Albedo ("Albedo (RGB)", 2D) = "white" {}
		_Gloss ("Specular", 2D) "grey" {}
		_Disp ("Displacement", 2D) "grey" {}
		_Bump ("Normal map", 2D) = "bump" {}
	}

	SubShader
	{
		Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue"="Transparent" }
		
		CGINCLUDE
		uniform fixed4 _Albedo;
		uniform float4 _Albedo_ST;
		uniform fixed4 _Color;
		uniform fixed4 _Gloss;
		uniform fixed4 _Bump;

		struct input
		{

		};
		struct v2f 
		{
			
		};

		v2f vert ( input i )
		{
			
		};

		ENDCG

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fargment frag
			#pragma target 3.0


			ENDCG
		}
	}
	FallBack "Diffuse"
}

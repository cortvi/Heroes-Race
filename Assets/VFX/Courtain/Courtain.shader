// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Adapted by @cortvi

Shader "Hidden/Cortinilla"
{
    Properties
    {
        [PerRendererData]
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Cells ("Background celling (XY) & spacing (ZW)", Vector) = (10, 10, 0, 0)
		_CTex ("Cortinilla Texture", 2D) = "white" {}
        _FColor ("Fore color", Color) = (1,1,1,1)
		_BColor ("Back color", Color ) = (0,0,0,0)
		_Offset ("Center (XY) & BG speed (ZW)", Vector) = (0,0,0,0)
		_Fade ("Scale", Range(0.0, 1.0)) = 1.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;

                return OUT;
            }

            fixed4 _FColor;
			fixed4 _BColor;
            sampler2D _MainTex;
			sampler2D _CTex;
			float4 _Cells;
			float4 _Offset;
			float _Fade;

            fixed4 frag(v2f IN) : SV_Target
            {
				// Modify background UVs
				float2 bgCoords = IN.texcoord;
				bgCoords += _Time.y * _Offset.zw;
				float col = floor (bgCoords * _Cells.x % 2);
				bgCoords.x = frac ( bgCoords.x * _Cells.x );
				bgCoords.y += lerp (0, _BColor.a, col);
				bgCoords.y = frac ( bgCoords.y * _Cells.y );
				bgCoords = (bgCoords - 0.5) * _Cells.zw + 0.5;

				// Modify cortinilla UVs
				float2 coords = IN.texcoord;
				_Fade = pow ( _Fade, 3.0 );
				coords *= (10.0 * _Fade);
				coords += 0.5 - _Offset * (_Fade * 10.0);

                half cAlpha = tex2D(_CTex, coords).a;
				half bgAlpha = tex2D (_MainTex, bgCoords).a;
				half4 color = half4 (lerp(_BColor, _FColor, bgAlpha * _FColor.a).rgb, cAlpha);

				// Modify Alpha
				float alphaAdd = smoothstep (0.7, 1.0, _Fade);
				color.a = saturate(color.a + alphaAdd);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

//				return half4(col.xxx, 1);
                return color;
            }
			ENDCG
        }
    }
}
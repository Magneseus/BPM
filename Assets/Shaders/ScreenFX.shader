Shader "Hidden/ScreenFX"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BeatTime ("Beat Time", Float) = 0
		_UpBeat ("Up Beat", Int) = 1
		_PulseScale ("Pulse Scale", Float) = 100
		_CameraAngle ("Camera Angl", Vector) = (0,0,0,0)
		_ScreenSize ("Screen Size", Vector) = (800, 600, 0, 0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _BeatTime;
			int _UpBeat;
			float _PulseScale;
			float3 _CameraAngle;
			fixed2 _ScreenSize;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float pi = 3.14159;
				float maxDistort = 0.6f;

				// ===========================================
				// COLOR PULSING
				// ===========================================

				// scale radius
				// yscale = _CameraAngle.x / 90
				float2 centerScreen;
				centerScreen.x = _ScreenSize.x/2;
				centerScreen.y = _ScreenSize.y/2;
				float yscale = _CameraAngle.x / 90;
				float difference = length(i.vertex - centerScreen);
				float distort = (maxDistort * (centerScreen.y - i.vertex.y) / (_ScreenSize.y/2)) + 1;
				float radius = centerScreen.x * _PulseScale / 2;
				difference -= _BeatTime * centerScreen.x * pi * (0.65 - _BeatTime);

				fixed4 pink = fixed4(255, 80, 200, 0);
				fixed4 indigo = fixed4(100, 50, 255, 0);
				//float maxColor = 0.0005;
				float maxColor = 0.0005;
				pink *= maxColor;
				indigo *= maxColor;

				float sinCurve = sin((difference*distort)/radius);
				float colorAmount = abs(sinCurve);

				if(sinCurve > 0){

					col += (pink * colorAmount);
				} else {
					col += (indigo * colorAmount);
				}

				// ===========================================
				// CONTRAST
				// ===========================================

				//col.x *= sin(col.x);
				//col.y *= sin(col.y);
				//col.z *= sin(col.z);


				return col;
			}

			ENDCG
		}
	}
}

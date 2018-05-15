Shader "EMPlus/DomeFisheyeShader"
{
	Properties
	{
		_LeftTex ("Texture", 2D) = "red" {}
		_RightTex ("Texture", 2D) = "green" {}
		_TopTex ("Texture", 2D) = "black" {}
		_BottomTex ("Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ RECT_ON

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _LeftTex;
			sampler2D _RightTex;
			sampler2D _TopTex;
			sampler2D _BottomTex;
			float4 _LeftTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = float4(1,1,0,1);//tex2D(_LeftTex, i.uv);
#if RECT_ON
				float2 uv;
				float2 angle = (i.uv + float2(+0.25, -0.5)) * UNITY_PI;
				float x = sin(angle.x) * cos(angle.y);
				float y = sin(angle.y);
				float z = cos(angle.x) * cos(angle.y);
#else
				float2 uv = (i.uv + float2(-0.5, -0.5)) * 2.0;
				float luv = length(uv);
				if (luv > 1.0) discard;
				float2 angle = float2(atan2(uv.y, uv.x) + UNITY_PI * 0.5, (1.0 - luv) * UNITY_PI * 0.5);

				float x = sin(angle.x) * cos(angle.y);
				float z = -sin(angle.y);
				float y = -cos(angle.x) * cos(angle.y);
				float3 coord = float3(x, y, z);
				float a = sqrt(0.5);
				float3x3 rot = float3x3(-a, 0, -a, 0, 1, 0, -a, 0, a);
				float3 coordr = mul(coord, rot);
				x = coordr.x; y = coordr.y; z = coordr.z;
#endif
				float absX = abs(x);
				float absY = abs(y);
				float absZ = abs(z);
				float maxAxis, uc, vc;

				// POSITIVE X
				if (x > 0 && absX >= absY && absX >= absZ) {
					maxAxis = absX;
					uc = -z;
					vc = y;
					uv.x = 0.5f * (uc / maxAxis + 1.0f);
					uv.y = 0.5f * (vc / maxAxis + 1.0f);
					return tex2D(_LeftTex, uv);
				}
				// POSITIVE Y
				else if (y > 0 && absY >= absX && absY >= absZ) {
					maxAxis = absY;
					uc = x;
					vc = -z;
					uv.y = 1.0f - 0.5f * (uc / maxAxis + 1.0f);
					uv.x = 0.5f * (vc / maxAxis + 1.0f);
					return tex2D(_TopTex, uv);
				}
				// NEGATIVE Y
				else if (y <= 0 && absY >= absX && absY >= absZ) {
					maxAxis = absY;
					uc = x;
					vc = z;
					uv.y = 1.0 - 0.5f * (uc / maxAxis + 1.0f);
					uv.x = 1.0 - 0.5f * (vc / maxAxis + 1.0f);
					return tex2D(_BottomTex, uv.yx);
				}
				// NEGATIVE Z
				else if (z <= 0 && absZ >= absX && absZ >= absY) {
					maxAxis = absZ;
					uc = -x;
					vc = y;
					uv.x = 0.5f * (uc / maxAxis + 1.0f);
					uv.y = 0.5f * (vc / maxAxis + 1.0f);
					return tex2D(_RightTex, uv);
				}

				return col;
			}
			ENDCG
		}
	}
}

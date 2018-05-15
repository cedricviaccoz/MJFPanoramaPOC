Shader "Unlit/Dome2ProjectorShader"
{
	Properties
	{
		_LeftTex ("Texture", 2D) = "red" {}
		_RightTex ("Texture", 2D) = "green" {}
		_TopTex ("Texture", 2D) = "white" {}
		_BottomTex ("Texture", 2D) = "blue" {}
		_BlendWidth ("Blend Width", float) = 10.0
		_BlendPower ("Blend Function Power", float) = 1.0
		_BlendGamma ("Blend Gamma (0:off, sign:hemi)", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ GRID_ON

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 localVertex : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _LeftTex;
			sampler2D _RightTex;
			sampler2D _TopTex;
			sampler2D _BottomTex;
			float _BlendWidth;
			float _BlendPower;
			float _BlendGamma;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.localVertex = v.vertex;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col;
				float2 uv;

				float x = i.localVertex.x;
				float y = i.localVertex.y;
				float z = i.localVertex.z;
				float absX = abs(x);
				float absY = abs(y);
				float absZ = abs(z);
				float maxAxis, uc, vc;
				float lat = acos(y) / UNITY_PI;

				// CALC BLEND REGION
				float g;
				float sgamma = sign(_BlendGamma);
				if (sgamma == 0) {
					g = 1;
				} else {
					float bw = _BlendWidth / 180.0;
					g = saturate((sgamma * (lat -  0.5) + bw) / (bw*2.0));
#if GRID_ON
					g = (g > 0 && g < 1 ? 0.5 : g);
#else
					g = (g < 0.5 ? 0.5 * pow(2.0 * g, _BlendPower) : 1.0 - 0.5 * pow(2.0 * (1.0 - g), _BlendPower));
					g = pow(g, abs(_BlendGamma));
#endif
				}
				float4 blendCol = float4(g, g, g, 1);

				// POSITIVE X
                if (x > 0 && absX >= absY && absX >= absZ) {
                    //	index = 0;
                    maxAxis = absX;
                    uc = -z;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
#if GRID_ON
                    col = lerp(tex2D(_LeftTex, uv), float4(0, 0, 0, 1), step(0.9, sin(-z * 200.0 + _Time.y * 16.0))*0.2);
#else
                    return tex2D(_LeftTex, uv) * blendCol;
#endif
                }
                // POSITIVE Y
                else if (y > 0 && absY >= absX && absY >= absZ) {
                    //	index = 2;
                    maxAxis = absY;
                    uc = x;
                    vc = -z;
                    uv.y = 1.0f - 0.5f * (uc / maxAxis + 1.0f);
                    uv.x = 0.5f * (vc / maxAxis + 1.0f);
                    if (uv.x < uv.y) discard;
#if GRID_ON
                    col = lerp(tex2D(_TopTex, uv), float4(0, 0, 0, 1), step(0.9, sin(y * 200.0 + _Time.y * 16.0))*0.2);
                    //					col = tex2D(_TopTex, uv);
#else
                    return tex2D(_TopTex, uv) * blendCol;
#endif
                }
                // NEGATIVE Y
                else if (y <= 0 && absY >= absX && absY >= absZ) {
                    //	index = 3;
                    maxAxis = absY;
                    uc = x;
                    vc = z;
                    uv.y = 1.0 - 0.5f * (uc / maxAxis + 1.0f);
                    uv.x = 1.0 - 0.5f * (vc / maxAxis + 1.0f);
                    if (uv.x < uv.y) discard;
#if GRID_ON
                    //					col = lerp(tex2D(_BottomTex, uv), float4(0,1,1,1), 0.2);
                    col = tex2D(_BottomTex, uv.yx);
#else
                    return tex2D(_BottomTex, uv.yx) * blendCol;
#endif
                }
                // NEGATIVE Z
                else if (z <= 0 && absZ >= absX && absZ >= absY) {
                    //	index = 5;
                    maxAxis = absZ;
                    uc = -x;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
#if GRID_ON
                    col = lerp(tex2D(_RightTex, uv), float4(0, 0, 0, 1), step(0.9, sin(x * 200.0 + _Time.y * 16.0))*0.2);
                    //					col = tex2D(_RightTex, uv);
#else
                    return tex2D(_RightTex, uv) * blendCol;
#endif
                }
                /*
                // NEGATIVE X
                else if (x <= 0 && absX >= absY && absX >= absZ) {
                    //	index = 1;
                    discard;
                    maxAxis = absX;
                    uc = z;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
                }
                // POSITIVE Z
                else if (z > 0 && absZ >= absX && absZ >= absY) {
                    //	index = 4;
                    discard;
                    maxAxis = absZ;
                    uc = x;
                    vc = y;
                    uv.x = 0.5f * (uc / maxAxis + 1.0f);
                    uv.y = 0.5f * (vc / maxAxis + 1.0f);
                }
                */
                else {
#if GRID_ON
                    col = float4(1, 0, 0, 1);
#else
                    discard;
#endif
                }

				#if GRID_ON
				float lon = atan2(z, x) / UNITY_PI;
				float nlon = smoothstep(0.0, 0.05, abs(lon + 0.25));
				float nlat = smoothstep(0.0, 0.05, abs(lat - 0.5));
				col = lerp(col, float4(1,nlon,1,1), smoothstep(0.47, 0.5, abs(0.5 - frac(lon * 18.0 + 18.5))));
				col = lerp(col, float4(nlat,1,1,1), smoothstep(0.47, 0.5, abs(0.5 - frac(lat * 18.0 + 9))));
				col *= blendCol;
				#else
				discard;
				#endif
				return col;
			}
			ENDCG
		}
	}
}

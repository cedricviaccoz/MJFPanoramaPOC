Shader "EMPlus/LongLatShader"
{
	Properties
	{
        _LineWidth("Line Width", Range(0.0, 0.2)) = 0.05
        _Color("Color", Color) = (1,1,1,1)
    }
	SubShader
	{
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 localVertex : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _LineWidth;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localVertex = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.localVertex.x;
                float y = i.localVertex.y;
                float z = i.localVertex.z;
                float lat = acos(y) / UNITY_PI;
                float lon = atan2(z, x) / UNITY_PI;
                float onlon = step(0.5 * (1.0 - _LineWidth), abs(0.5 - frac(lon * 18.0 + 18.0)));
                float onlat = step(0.5 * (1.0 - _LineWidth), abs(0.5 - frac(lat * 18.0 + 18.0)));
                if (max(onlon, onlat) < 0.1) {
                    discard;
                }
                return lerp(float4(0,0,0,0), _Color, max(onlon, onlat));
			}
			ENDCG
		}
	}
}

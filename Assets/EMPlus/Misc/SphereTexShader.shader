Shader "EMPlus/SphereTexShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "gray" {}
        _Fade("Fade", float) = 1
        _ShiftU("Shift U", float) = 0
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
			
			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 orig : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ShiftU;
            float _Fade;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.orig = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float lat = acos(-i.orig.y) / UNITY_PI;
                float lon = atan2(i.orig.z, -i.orig.x) / UNITY_PI;
                float2 uv = float2(lon * 0.5, (lat - 0.5) + 0.5);
                return tex2D(_MainTex, frac(uv + float2(_ShiftU, 0.0))) * float4(_Fade, _Fade, _Fade, _Fade);
            }
            ENDCG
		}
	}
}

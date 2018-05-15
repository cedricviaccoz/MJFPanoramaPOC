Shader "EMPlus/PanoramaSliceWarpShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _SliceLayout("Slice Layout (number, angle(rad), px.x, px.y)", Vector) = (1, 1.57, 1024, 1024)
        _ShiftY("Camera Shift Y", float) = 0
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float4 _SliceLayout;
            float _ShiftY;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                float sliceFrac     = frac(i.uv.x * _SliceLayout.x);
                float sliceOffset   = i.uv.x - sliceFrac / _SliceLayout.x;
                float maxTan = tan(_SliceLayout.y * 0.5);

                float sliceAngle = (sliceFrac - 0.5) * _SliceLayout.y;
                float u = (tan(sliceAngle) / maxTan + 1.0) / 2.0;
                float dist = sqrt(pow(tan(sliceAngle), 2.0) + 1.0) / sqrt(1.0 + maxTan*maxTan);
//                if (i.uv.y > 0.5)
                {
                    i.uv.x = u / _SliceLayout.x;
                    i.uv.y = (i.uv.y - 0.5 + _ShiftY) * dist + 0.5 - _ShiftY;
                    i.uv.x += sliceOffset;
                }
                if (i.uv.x < 0.0 || i.uv.x > 1.0)
            		discard;
				fixed4 col = tex2D(_MainTex, i.uv);
                if (i.uv.y > 1.0 || i.uv.y < 0.0)
                    col = fixed4(1, 0, 1, 1);
//                col.r = lerp(sliceFrac, 1.0, step(0.5, frac(i.uv.y * 20.0)));
                return col;
			}
			ENDCG
		}
	}
}

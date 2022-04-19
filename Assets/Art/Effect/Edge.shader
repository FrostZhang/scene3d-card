Shader "Hidden/Edge"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Edge("_Edge", Color) = (1,1,1,1)
		_W("W", float) = 0.01
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite On ZTest Off

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

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				fixed4 _Edge;
				float _W;
				fixed4 frag(v2f i) : SV_Target
				{
					float a1 = step(_W ,  i.uv.x);
					float a2 = step(_W, i.uv.y);
					float a3 = step(i.uv.x, 1 - _W);
					float a4 = step(i.uv.y,  1 - _W);
					clip(1 - a1 * a2*a3*a4 - 0.01);
					return _Edge;
				}
				ENDCG
			}
		}
}

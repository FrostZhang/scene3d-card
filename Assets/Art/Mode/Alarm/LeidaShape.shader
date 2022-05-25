Shader "Unlit/LeidaShape"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DepthColor("Depth Color",COLOR)=(1,0,0,1)
		_Threshold("Threshold",float )=0.5
		_Rim("Rim",float )=1
		_Color("Color",COLOR)=(0,0,0,1)

	}
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Cull Off Lighting Off ZTest LEqual
		Blend  SrcAlpha OneMinusSrcAlpha
		Pass
		{
			//Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile_particles

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				float4 tangent:TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 projpos :TEXCOORD2;
				float3 normal : NORMAL;
				float3 viewdir:TEXCOORD3;
				float3 lightdir:TEXCOORD4;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			v2f vert (appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				v2f o;
				v.vertex.xyz *= clamp((frac(_Time.y)+1)*0.5,0.5,1);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.projpos = ComputeScreenPos(o.vertex);	//projectedPosition 为了frag获得该物体在深度图的位置
				COMPUTE_EYEDEPTH(o.projpos.z);	//获得此物体的深度
				return o;
			}

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			fixed4 _DepthColor;
			fixed4 _Color;
			float _Threshold;
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);

				float2 uv =i.uv;
				fixed4 col = tex2D(_MainTex, uv) *_DepthColor;

				float sz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(i.projpos )));
				float fade = saturate(_Threshold*(sz - i.projpos.z));
				float intersect = (1 - fade) ;
				col = lerp(col,_Color,intersect);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

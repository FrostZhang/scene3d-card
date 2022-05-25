Shader "Hidden/FontBillboard"
{
    Properties
    {
        //[PerRendererData]
        _MainTex ("Texture", 2D) = "white" {}
        [MaterialToggle]_Verical("Vercial",Range(0,1))=1
        _Flip("Flip",Range(-1,1))=1
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityUI.cginc"
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color    : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			fixed _Verical;
            fixed4 _Color;
            fixed _Flip;
            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v); 
                UNITY_INITIALIZE_OUTPUT(v2f, o); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

				float3 center = float3(0,0,0);
				//视角方向：摄像机的坐标减去物体的点
				float3 view = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1));
				float3 normalDir = view - center;
				//表面法线的变化：如果_Verical=1，则为表面法线，否则为向上方向
				normalDir.y = normalDir.y*_Verical;
				//归一化
				normalDir = normalize(normalDir);
				float3 upDir = abs(normalDir.y) > 0.999 ? float3(0, 0, 1) : float3(0, 1, 0);
				//叉乘  cross(A,B)返回两个三元向量的叉积(cross product)。注意，输入参数必须是三元向量
				float3 rightDir = normalize(cross(upDir,normalDir));
				upDir = normalize(cross(normalDir, rightDir));
				//计算中心点偏移
				float3 centerOffs = v.vertex.xyz - center;
				//位置的变换
				float3 localPos = center + rightDir * centerOffs.x * _Flip + upDir * centerOffs.y + normalDir * centerOffs.z;
                o.vertex = UnityObjectToClipPos(localPos);
                //o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif
                col.rgb += i.color * col.a;
                return col;
            }
            ENDCG
        }
    }
}

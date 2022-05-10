Shader "Unlit/UIColorPickH"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "HsvRgb.cginc"

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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float2 d = i.uv * 2 - 1;
                float l = length(d);
                float a = smoothstep(0.73,0.74, l) - smoothstep(0.99,1, l);
                float2 p = normalize(d);

                float theta = acos(dot(float2(1, 0), p));
                if(p.y < 0)
                    theta = UNITY_TWO_PI - theta;
                
                float3 rgb = hsv2rgb(float3(theta * UNITY_INV_TWO_PI, 1, 1));
                return fixed4(rgb,a);
            }
            ENDCG
        }
    }
}

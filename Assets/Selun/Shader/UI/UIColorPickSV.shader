Shader "Unlit/UIColorPickSV"
{
    Properties
    {
        _Hue("Hue", Range(0, 360)) = 0
		_MainTex("MainTex",2D) = "white" {}
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

            float _Hue;
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

            // sdf实现三角形
            // https://iquilezles.org/www/articles/distfunctions2d/distfunctions2d.htm
            // https://www.shadertoy.com/view/XsXSz4
            float sdTriangle( float2 p, float2 p0, float2 p1, float2 p2 )
            {
                float2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
                float2 v0 = p -p0, v1 = p -p1, v2 = p -p2;
                float2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
                float2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
                float2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
                float s = sign( e0.x*e2.y - e0.y*e2.x );
                float2 d = min(min(float2(dot(pq0,pq0), s*(v0.x*e0.y-v0.y*e0.x)),
                                float2(dot(pq1,pq1), s*(v1.x*e1.y-v1.y*e1.x))),
                                float2(dot(pq2,pq2), s*(v2.x*e2.y-v2.y*e2.x)));
                return -sqrt(d.x)*sign(d.y);
            }

            // 判定三角形内
            float inTriangle(float2 p, float2 p0, float2 p1, float2 p2 )
            {
                float2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
                float2 v0 = p -p0, v1 = p -p1, v2 = p -p2;

                float3 a = cross(float3(e0,0), float3(v0,0));
                float3 b = cross(float3(e1,0), float3(v1,0));
                float3 c = cross(float3(e2,0), float3(v2,0));
                return all(float2(saturate(sign(dot(a, b))), saturate(sign(dot(b, c)))));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算三角形内
                float sqrt3dv2 = 0.8660254037844386; // sqrt(3)/2
                float oneminus = 1-sqrt3dv2;

                float2 a = float2(0,1);
                float2 b = float2(1,1);
                float2 c = float2(0.5, oneminus);
                // float d = sdTriangle(i.uv, a, b, c);
                // float alpha = 1.0-smoothstep(0.0,0.001,d);                
                float d = inTriangle(i.uv, a, b, c);
                float alpha = smoothstep(0.0,0.001,d);

                // sample the texture
                // 求 y 对应的x的宽度
                float xwidth = (i.uv.y - oneminus) / sqrt3dv2; // 发现这个和下面的y的计算居然是一样的
                float s = i.uv.x - 0.5+xwidth/2;
                // float3 rgb = hsv2rgb(float3(_Hue/360, s/xwidth, (i.uv.y-oneminus)/sqrt3dv2));
                float3 rgb = hsv2rgb(float3(_Hue/360, s/xwidth, xwidth));
                return fixed4(rgb, alpha);
            }
            ENDCG
        }
    }
}

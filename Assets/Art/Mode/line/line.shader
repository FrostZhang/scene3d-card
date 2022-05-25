Shader "Unlit/line"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        _Speed("Speed",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color: COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color: COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float _Speed;
            fixed4 _Color;
            fixed4 _EmissionColor;
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                i.uv.x +=_Time.y*_Speed;
                fixed4 col = tex2D(_MainTex, i.uv) * i.color *  (1+_EmissionColor);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

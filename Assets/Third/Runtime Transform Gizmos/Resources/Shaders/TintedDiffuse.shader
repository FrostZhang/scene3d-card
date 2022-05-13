// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TintedDiffuse" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("MainTexture", 2D) = "white" {}
		_ZWrite ("ZWrite", int) = 1
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent"  "Queue"="Transparent" }

			Blend SrcAlpha OneMinusSrcAlpha
			ZTest On
			Cull Off
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _Color;

			struct vInput 
			{
				float4 vertexPos : POSITION;
				float2 vertexUV : TEXCOORD0;
			};

			struct vOutput
			{
				float4 clipPos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			vOutput vert(vInput input)
			{
				vOutput o;
				o.clipPos = UnityObjectToClipPos(input.vertexPos);
				o.uv = input.vertexUV;

				return o;
			}

			float4 frag(vOutput input) : COLOR
			{		
				return tex2D(_MainTex, input.uv) * _Color;
			}
			ENDCG
		}
	}
}

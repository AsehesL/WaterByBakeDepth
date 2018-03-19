// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
//深度图渲染Shader
Shader "Hidden/DepthMapRenderer"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform float depth;
			uniform float power;
			uniform float height;
			uniform float minheight;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 depth:TEXCOORD0;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

				float h = worldPos.y - height;
				o.depth = float2(saturate(-h / minheight), step(h, 0));

				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float dp = pow(saturate(i.depth.x / depth),power);
				return float4(dp,i.depth.y,dp,1);
			}
			ENDCG
		}
		Pass{
			cull front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 vert(float4 v:POSITION) :SV_POSITION{
				return UnityObjectToClipPos(v);
			}

			fixed4 frag(float4 p:SV_POSITION) : SV_TARGET{
				return fixed4(0,0,0,1);
			}

			ENDCG
		}
	}
}

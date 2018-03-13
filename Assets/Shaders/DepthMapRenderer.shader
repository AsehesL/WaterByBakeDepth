// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DepthMapRenderer"
{
	Properties
	{
	}
	CGINCLUDE

	uniform float depthR;
	uniform float powerR;
	uniform float depthG;
	uniform float powerG;
	uniform float depthB;
	uniform float powerB;
	uniform float depthA;
	uniform float powerA;

	fixed4 GetDepth(float dp) {
		fixed4 dep;
		dep.r = pow(saturate(dp / depthR), powerR);
		dep.g = pow(saturate(dp / depthG), powerA);
		dep.b = pow(saturate(dp / depthB), powerG);
		dep.a = pow(saturate(dp / depthA), powerB);
		return dep;
	}

	ENDCG
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float depth:TEXCOORD0;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth = COMPUTE_DEPTH_01;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return GetDepth(i.depth);
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

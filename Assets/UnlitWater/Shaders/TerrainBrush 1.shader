// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/MeshPainter/Editor/TerrainBrush"
{
	Properties
	{
		_Color ("Color", color) = (1,1,1,1)
		_VIndex("VIndex", vector) = (0,0,0,0)
		_UVScale("UVScale", vector) = (0, 0, 0, 0)
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"

	struct appdata_point
	{
		float4 vertex : POSITION;
		uint vid : SV_VertexID;
	};

	struct v2f_point
	{
		float4 vertex : SV_POSITION;
		float4 ap:TEXCOORD0;
	};

	struct v2f_projector
	{
		float4 vertex : SV_POSITION;
		float4 proj : TEXCOORD0;
	};

	float4x4 internalWorld2BrushProjectorn;
	float4 _VIndex;
	half4 _Color;

	v2f_point vert_point(appdata_point v)
	{
		v2f_point o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		float nk0 = 1 - saturate(step(abs(v.vid - _VIndex.x), 0.001));
		float nk1 = 1 - saturate(step(abs(v.vid - _VIndex.y), 0.001));
		float nk2 = 1 - saturate(step(abs(v.vid - _VIndex.z), 0.001));
		o.ap = nk0*nk1*nk2;
		return o;
	}

	fixed4 frag_point(v2f_point i) : SV_Target
	{
		fixed4 col = _Color;
		col.a *= 1 - i.ap;

		return col;
	}

	v2f_point vert_triangle(appdata_point v)
	{
		v2f_point o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		float nk0 = 1 - saturate(step(abs(v.vid - _VIndex.x), 0.001));
		o.ap = nk0;
		return o;
	}

	fixed4 frag_triangle(v2f_point i) : SV_Target
	{
		fixed4 col = _Color;
		col.a *= 1 - i.ap;

		return col;
	}

	v2f_projector vert_projector(float4 v:POSITION)
	{
		v2f_projector o;
		o.vertex = UnityObjectToClipPos(v);
		o.proj = mul(internalWorld2BrushProjectorn, mul(unity_ObjectToWorld, v));
		return o;
	}

	fixed4 frag_projector(v2f_projector i) : SV_Target
	{
		float3 pj = i.proj.xyz / i.proj.w;

		float l = step(length(pj.xy), 1);

		fixed4 col = _Color;
		/*if (pj.x < -1 || pj.x>1 || pj.y < -1 || pj.y>1)
		col.a = 0;*/
		col.a *= l;
		//col.rgb = _Color.rgb;
		return col;
		//return fixed4(pj, 1);
	}

	ENDCG
	SubShader
	{
		Tags{ "ForceSupported" = "True" }
		Pass{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_point
			#pragma fragment frag_point
			#pragma target 3.5
			ENDCG
		}
		Pass{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_triangle
			#pragma fragment frag_triangle
			#pragma target 3.5
			ENDCG
		}
		Pass
		{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_projector
			#pragma fragment frag_projector
			ENDCG
		}
	}
	SubShader{
		Tags{ "ForceSupported" = "True" }
		Pass{
			zwrite off
			colormask 0
		}
		Pass{
			zwrite off
			colormask 0
		}
		Pass
		{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_projector
			#pragma fragment frag_projector
			ENDCG
		}
	}
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hidden/MeshPainter/Editor/TerrainBrush"
{
	Properties
	{
		_Color ("Color", color) = (1,1,1,1)
		_BrushTex ("Brush", 2D) = "white" {}
		_VIndex("VIndex", vector) = (0,0,0,0)
		_UVScale("UVScale", vector) = (0, 0, 0, 0)
	}
	CGINCLUDE
	
	#include "UnityCG.cginc"

	struct appdata_brush
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct appdata_point
	{
		float4 vertex : POSITION;
		uint vid : SV_VertexID;
	};

	struct v2f_brush
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
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

	float4x4 internalUV2BrushMatrix;
	float4x4 internalWorld2BrushProjectorn;
	float4 _VIndex;
	half4 _Color;
	half4 _UVScale;
	sampler2D _BrushTex;

	v2f_brush vert_brush(appdata_brush v)
	{
		v2f_brush o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	fixed4 frag_brush(v2f_brush i) : SV_Target
	{
#if UNITY_UV_STARTS_AT_TOP
		//i.uv.y = 1 - i.uv.y;
#endif
		float4 uv = mul(internalUV2BrushMatrix, float4(i.uv + _UVScale.xy,0,1));
		uv.xy /= uv.w;
		fixed4 col = tex2D(_BrushTex, uv.xy);
		if (uv.x < 0 || uv.x>1 || uv.y < 0 || uv.y>1)
			col.a = 0;
		col.rgb = _Color.rgb;
		col.a *= _Color.a;
		return col;
	}

	fixed4 frag_pointbrush(v2f_brush i) : SV_Target
	{
#if UNITY_UV_STARTS_AT_TOP
		//i.uv.y = 1 - i.uv.y;
#endif
		float4 uv = mul(internalUV2BrushMatrix, float4(i.uv,0,1));
		uv.xy /= uv.w;
		float l = length(uv);
		float d = step(l,0.1);
		fixed4 col = _Color;
		col.a *= d;

		return col;
	}

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
		Pass
		{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_brush
			#pragma fragment frag_brush
			ENDCG
		}
		Pass{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_brush
			#pragma fragment frag_pointbrush
			ENDCG
		}
		Pass{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_point
			#pragma fragment frag_point
			#pragma target 3.5
			#pragma exclude_renderers d3d9
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
			#pragma exclude_renderers d3d9
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
		Pass
		{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_brush
			#pragma fragment frag_brush
			ENDCG
		}
		Pass{
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_brush
			#pragma fragment frag_pointbrush
			ENDCG
		}
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

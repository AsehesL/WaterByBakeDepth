//笔刷Shader
Shader "Hidden/MeshPainter/Editor/TerrainBrush"
{
	Properties
	{
		_Color ("Color", color) = (1,1,1,1)
		_VIndex("VIndex", vector) = (0,0,0,0)
		_UVScale("UVScale", vector) = (0, 0, 0, 0)
		_VertexMask("VertexMask", vector) = (0,0,0,0)
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

	struct v2f_preview
	{
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
	};

	float4x4 internalWorld2BrushProjectorn;
	float4 _VIndex;
	half4 _Color;
	half4 _VertexMask;

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

	v2f_preview vert_preview(float4 v:POSITION, float4 col:COLOR)
	{
		v2f_preview o;
		o.vertex = UnityObjectToClipPos(v);
		o.color = col;
		return o;
	}

	fixed4 frag_preview(v2f_preview i) : SV_Target
	{
		fixed r = i.color.r*_VertexMask.r;
		fixed g = i.color.g*_VertexMask.g;
		fixed b = i.color.b*_VertexMask.b;
		fixed a = i.color.a*_VertexMask.a;

		fixed c = r + g + b + a;
		return fixed4(c, c, c, 1);
	}

	ENDCG
	SubShader
	{
		Tags{ "ForceSupported" = "True" }
		Pass{
			//该Pass绘制顶点笔刷
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
			//该Pass绘制三角笔刷
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
			//该Pass绘制顶点色预览
			blend srcalpha oneminussrcalpha
			offset -1,-1
			zwrite off
			CGPROGRAM
			#pragma vertex vert_preview
			#pragma fragment frag_preview
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
			#pragma vertex vert_preview
			#pragma fragment frag_preview
			ENDCG
		}
	}
}

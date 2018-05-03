Shader "Hidden/EdgeBlur"
{
	Properties
	{
		_Offset ("Offset", float) = 0
		_Mix ("Mix", 2D) = "black" {}
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			half _Offset;
			sampler2D _MainTex;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv;
				o.uv.zw = float2(_Offset, 0);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv.xy)*0.5;
				col += tex2D(_MainTex, i.uv.xy + i.uv.zw)*0.175;
				col += tex2D(_MainTex, i.uv.xy - i.uv.zw)*0.175;
				col += tex2D(_MainTex, i.uv.xy + i.uv.zw * 2)*0.075;
				col += tex2D(_MainTex, i.uv.xy - i.uv.zw * 2)*0.075;
				
				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			half _Offset;
			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.uv;
				o.uv.zw = float2(0, _Offset);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv.xy)*0.5;
				col += tex2D(_MainTex, i.uv.xy + i.uv.zw)*0.175;
				col += tex2D(_MainTex, i.uv.xy - i.uv.zw)*0.175;
				col += tex2D(_MainTex, i.uv.xy + i.uv.zw * 2)*0.075;
				col += tex2D(_MainTex, i.uv.xy - i.uv.zw * 2)*0.075;

				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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

			sampler2D _Mix;
			sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				float4 mix = tex2D(_Mix, i.uv);

				col.r = mix.g;
				col.b = mix.g;

				return col;
			}
			ENDCG
		}
	}
}

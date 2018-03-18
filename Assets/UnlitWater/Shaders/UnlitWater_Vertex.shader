// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/UnlitWater_Vertex"
{
	Properties
	{
		_NormalTex("NormalTex", 2D) = "black" {}
		_WaveTex("WaveTex", 2D) = "white" {}
		_Gradient("Gradient", 2D) = "white" {}
		_SineLength("SineLength", float) = 0
		_SineHeight("SineHeight", float) = 0
		//_SineNoise("SineNoise", float) = 0
		_WaveNoise("WaveNoise", float) = 0
		_WaveRange("WaveRange", float) = 0
		_VertexHeight("VertexHeight", float) = 0
		_VertexLength("VertexLength", float) = 0
		_Spoondrift("Spoondrift", vector) = (0,0,0,0)
		_Speed("Speed", vector) = (0,0,0,0)
		_Specular("Specular", float) = 0
		_Gloss("Gloss", float) = 0
		_LightDir("LightDir", vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "true" }
		Pass
		{
			blend srcalpha oneminussrcalpha
			zwrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma target 3.0
	
			#include "UnityCG.cginc"

			struct v2f
			{
				float2 uv_NormalTex : TEXCOORD0;
				float2 uv_WaveTex : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 TW0:TEXCOORD3;
				float4 TW1:TEXCOORD4;
				float4 TW2:TEXCOORD5;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _Gradient;
			sampler2D _NormalTex;
			sampler2D _WaveTex;

			fixed _SineLength;
			fixed _SineHeight;
			//fixed _SineNoise;

			half4 _Speed;
			fixed _WaveRange;
			fixed _WaveNoise;
			float4 _NormalTex_ST;
			float4 _WaveTex_ST;

			half4 _Spoondrift;

			half _VertexHeight;
			half _VertexLength;

			half _Specular;
			fixed _Gloss;

			half4 _LightDir;

			v2f vert(appdata_full v)
			{
				v2f o;
				v.vertex.y += (sin(_Time.x*_Speed.y + v.vertex.x*_VertexLength) + sin(_Time.x*_Speed.y + v.vertex.z*_VertexLength))*0.5*_VertexHeight;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
				o.uv_WaveTex = TRANSFORM_TEX(v.texcoord, _WaveTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				o.TW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.TW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.TW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//half3 normalNoise = tex2D(_NormalTex,i.uv_NormalTex).rgb*2-1;
				half3 waveNoise = tex2D(_WaveTex, i.uv_WaveTex).rgb;
				i.uv_NormalTex.x += _Time.x*_Speed.x;
				half sinFactor = sin(_Time.x*_Speed.w + i.uv_NormalTex.x*_SineLength);
				i.uv_NormalTex.y += _SineHeight*sinFactor;
				fixed4 normalCol = tex2D(_NormalTex, i.uv_NormalTex);
				fixed3 normalG = normalCol.rgb * 2 - 1;

#if UNITY_UV_STARTS_AT_TOP
				fixed4 acol = tex2D(_Gradient, float2(i.color.r, 1));
#else
				fixed4 acol = tex2D(_Gradient, float2(i.color.r, 0));
#endif
				acol.a *= i.color.a;

				fixed wave1 = tex2D(_WaveTex, float2(i.color.r + _WaveRange*sin(_Time.x*_Speed.z + _WaveNoise*waveNoise.b), 0)).r;
				fixed wave2 = tex2D(_WaveTex, float2(i.color.r + _WaveRange*cos(_Time.x*_Speed.z + _WaveNoise*waveNoise.b), 0)).r;
				fixed waveAlpha = tex2D(_WaveTex, float2(i.color.r.r, 0)).g;

				fixed sfadein = 1 - saturate((_Spoondrift.x - i.color.r) / _Spoondrift.x);
				fixed sfadeout = 1 - saturate((i.color.r - _Spoondrift.y) / _Spoondrift.z);
				acol = lerp(acol, fixed4(1, 1, 1, 1), (wave1 + wave2)*waveAlpha*normalCol.a*i.color.b);
				acol = lerp(acol, fixed4(1, 1, 1, 1), sfadein*sfadeout *_Spoondrift.w*normalCol.a*i.color.g);

				half3 worldPos = half3(i.TW0.w, i.TW1.w, i.TW2.w);

				half3 worldNormal = fixed3(dot(i.TW0.xyz, normalG), dot(i.TW1.xyz, normalG), dot(i.TW2.xyz, normalG));

				half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				half3 h = normalize(viewDir - normalize(_LightDir.xyz));
				fixed ndh = max(0, dot(worldNormal, h));

				//acol += _Gloss*pow(ndh, _Specular*128.0)*fixed4(1, 1, 1, 1);
				acol += _Gloss*pow(ndh, _Specular*128.0)*fixed4(1, 1, 1, 1)*(1 - 0.9*abs(sinFactor));
				return acol;
			}
			ENDCG
		}
	}
}

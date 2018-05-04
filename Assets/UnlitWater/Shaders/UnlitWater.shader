Shader "Unlit/UnlitWater"
{
	Properties
	{
		_FoamTex("泡沫贴图(R:海浪泡沫,G:岸边泡沫,B:海浪扰动)", 2D) = "white" {}
		[Normal]_NormalTex("法线贴图", 2D) = "bump" {}
		_WaveMask ("海浪遮罩", 2D) = "white" {}
		_WaveTex("海浪渐变", 2D) = "white" {}
		_Gradient("海水颜色渐变", 2D) = "white" {}
		_Sky("反射天空盒", cube) = "" {}

		[Space]
		_WaveParams ("海浪参数(x:海浪范围,y:海浪偏移,z:海浪扰动,w:浪花泡沫扰动)", vector) = (0,0,0,0)
		_FoamParams("岸边泡沫参数(x:淡入,y:淡出,z:宽度,w:透明度)", vector) = (0,0,0,0)
		_Speed("速度参数(x:风速,y:海浪速度)", vector) = (0,0,0,0)

		[Space]
		_NormalScale ("法线缩放", range(0, 1)) = 1
		_Fresnel("菲涅尔系数", float) = 0
		
		_Specular("Specular", float) = 0
		_Gloss("Gloss", float) = 0
		_FoamColor ("泡沫颜色", color) = (1,1,1,1)
		_SpecColor ("高光颜色", color) = (0.4,0.4,0.4,1)
		_LightDir("光照方向", vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "true" }
		LOD 100

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
				float2 uv_FoamTex : TEXCOORD0;
				float2 uv_NormalTex : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 TW0:TEXCOORD3;
				float4 TW1:TEXCOORD4;
				float4 TW2:TEXCOORD5;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _FoamTex;
			float4 _FoamTex_ST;

			sampler2D _WaveTex;
			sampler2D _WaveMask;

			half4 _Speed;
			
			fixed4 _WaveParams;

			half _NormalScale;

			half4 _FoamParams;

			sampler2D _Gradient;
			sampler2D _NormalTex;
			float4 _NormalTex_ST;

			half _Fresnel;

			samplerCUBE _Sky;

			half _Specular;
			fixed _Gloss;

			half4 _LightDir;
			half4 _SpecColor;

			fixed4 _FoamColor;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv_FoamTex = TRANSFORM_TEX(v.texcoord, _FoamTex);
				o.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
				//o.uv_WaveTex = TRANSFORM_TEX(v.texcoord, _WaveTex);

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
			
			fixed4 frag (v2f i) : SV_Target
			{
				//采样法线贴图
				fixed4 normalCol = (tex2D(_NormalTex, i.uv_NormalTex + fixed2(_Time.x*_Speed.x, 0)) + tex2D(_NormalTex, fixed2(_Time.x*_Speed.x + i.uv_NormalTex.y, i.uv_NormalTex.x))) / 2;
			
				half3 worldNormal = UnpackNormal(normalCol);

				//泡沫使用法线贴图的rg进行扰动
				half3 foam = tex2D(_FoamTex, i.uv_FoamTex +worldNormal.xy*_WaveParams.w).rgb;
				
				worldNormal = lerp(half3(0, 0, 1), worldNormal, _NormalScale);
				worldNormal = normalize(fixed3(dot(i.TW0.xyz, worldNormal), dot(i.TW1.xyz, worldNormal), dot(i.TW2.xyz, worldNormal)));

				//根据顶点颜色r通道采样海水渐变
				fixed4 col = tex2D(_Gradient, float2(i.color.r, 0.5));
				
				//采样反射天空盒
				half3 worldPos = half3(i.TW0.w, i.TW1.w, i.TW2.w);

				half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				half3 refl = reflect(-viewDir, worldNormal);

				half vdn = saturate(pow(dot(viewDir, worldNormal), _Fresnel));

				col.rgb = lerp(texCUBE(_Sky, refl), col.rgb, vdn);


				//计算海浪和岸边泡沫
				fixed wave1 = tex2D(_WaveTex, float2(i.color.r + _WaveParams.y + _WaveParams.x*sin(_Time.x*_Speed.y + _WaveParams.z*foam.b), 0)).r;
				fixed wave2 = tex2D(_WaveTex, float2(i.color.r + _WaveParams.y + _WaveParams.x*cos(_Time.x*_Speed.y + _WaveParams.z*foam.b), 0)).r;
				fixed waveAlpha = tex2D(_WaveMask, float2(i.color.r, 0)).r;

				fixed sfadein = 1 - saturate((_FoamParams.x - i.color.r) / _FoamParams.x);
				fixed sfadeout = 1 - saturate((i.color.r - _FoamParams.y) / _FoamParams.z);

				col+= (_FoamColor - col)* (wave1 + wave2)*waveAlpha*foam.r*i.color.b;
				col += (_FoamColor - col)* sfadein*sfadeout *_FoamParams.w*foam.g*i.color.g;

				//计算高光
				half3 h = normalize(viewDir - normalize(_LightDir.xyz));
				fixed ndh = max(0, dot(worldNormal, h));

				col += _Gloss*pow(ndh, _Specular*128.0)*_SpecColor;
				
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				//应用顶点透明度
				col.a *= i.color.a;
				return col;
			}
			ENDCG
		}
	}
}

Shader "Unlit/UnlitWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("NormalTex", 2D) = "black" {}
		_Gradient("Gradient", 2D) = "white" {}
		_Sky("Sky", cube) = "" {}

		_Fresnel("Fresnel", float) = 0

		_Speed("Speed(x:wavespeed)", vector) = (0,0,0,0)

		//_Specular("Specular", float) = 0
		//_Gloss("Gloss", float) = 0
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
				float2 uv_MainTex : TEXCOORD0;
				float2 uv_NormalTex : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 TW0:TEXCOORD3;
				float4 TW1:TEXCOORD4;
				float4 TW2:TEXCOORD5;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			half4 _Speed;

			sampler2D _Gradient;
			sampler2D _NormalTex;
			float4 _NormalTex_ST;

			half _Fresnel;

			samplerCUBE _Sky;

			//half _Specular;
			//fixed _Gloss;

			//half4 _LightDir;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);

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
				fixed4 normalCol = (tex2D(_NormalTex, i.uv_NormalTex + fixed2(_Time.x*_Speed.x, 0)) + tex2D(_NormalTex, fixed2(_Time.x*_Speed.x + i.uv_NormalTex.y, i.uv_NormalTex.x))) / 2;
				half3 worldNormal = UnpackNormal(normalCol);
				worldNormal = normalize(fixed3(dot(i.TW0.xyz, worldNormal), dot(i.TW1.xyz, worldNormal), dot(i.TW2.xyz, worldNormal)));
#if UNITY_UV_STARTS_AT_TOP
				fixed4 col = tex2D(_Gradient, float2(i.color.r, 1));
#else
				fixed4 col = tex2D(_Gradient, float2(i.color.r, 0));
#endif

				half3 worldPos = half3(i.TW0.w, i.TW1.w, i.TW2.w);

				half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				half3 refl = reflect(-viewDir, worldNormal);

				//col = texCUBE(_Sky, refl);

				half vdn = saturate(pow(dot(viewDir, worldNormal), _Fresnel));

				col.rgb = lerp(texCUBE(_Sky, refl), col.rgb, vdn);
				//half3 h = normalize(viewDir - normalize(_LightDir.xyz));
				//fixed ndh = max(0, dot(worldNormal, h));

				//col += _Gloss*pow(ndh, _Specular*128.0)*fixed4(0.4, 0.4, 0.4, 1);
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

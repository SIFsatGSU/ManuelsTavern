Shader "Custom/Tessellation" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap ("Normal", 2D) = "bump" {}
		_DisplacementMap ("Displacement map", 2D) = "gray" {}
		_DisplacementAmount ("Displacement amount", FLOAT) = 1
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_TessellationAmount ("Tessellation amount",  INT) = 4
		_TessellationMinDistance ("Tessllation min distance", FLOAT) = 10
		_TessellationMaxDistance ("Tessllation max distance", FLOAT) = 25
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow fullforwardshadows vertex:vert tessellate:tessDistance
		#pragma addshadow
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "Tessellation.cginc"

		int _TessellationAmount;
		float _DisplacementAmount;
		float _TessellationMinDistance;
		float _TessellationMaxDistance;
		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _DisplacementMap;

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 color;
		};

		float4 tessDistance (appdata_full v0, appdata_full v1, appdata_full v2) {
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _TessellationMinDistance
					, _TessellationMaxDistance, _TessellationAmount);
		}

		void vert (inout appdata_full v) {
			float displacement = tex2Dlod(_DisplacementMap, float4(v.texcoord.xy, 0, 0)).r * _DisplacementAmount;
			v.vertex.xyz += v.normal * displacement;
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
			o.Albedo = c.rgb;

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

Shader "Custom/MeshExploder"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Smoothness("Smoothness", Range(0, 1)) = 0
		_Metallic("Metallic", Range(0, 1)) = 0
		_MainTex("Main Texture", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_BumpPower("Bump Power", Range(3, 0.01)) = 1 
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Cull Off

		CGPROGRAM

#pragma surface surf Standard vertex:vert addshadow
#pragma instancing_options procedural:setup
#pragma target 4.0
#include "UnityCG.cginc"
#include "SimplexNoiseGrad3D.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float4 texcoord1 : TEXCOORD1;
		float4 texcoord2 : TEXCOORD2;
		float4  color : COLOR;

		uint vid : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Input
	{
		float vface : VFACE;
		float2 customUV;
	};

	struct triangleMeshRaw
	{
		float4 triPos;
		float4 triNorms;
		float2 triUVs;
	};

	half4 _Color;
	half _Smoothness;
	half _Metallic;
	sampler2D _MainTex;
	sampler2D _BumpMap;
	float _BumpPower;

	float4x4 _LocalToWorld;
	float4x4 _WorldToLocal;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

	StructuredBuffer<triangleMeshRaw> _triBuffer;

#endif

	void vert(inout appdata v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

		uint id = unity_InstanceID * 3 + v.vid;
		v.vertex = _triBuffer[id].triPos;
		v.normal = _triBuffer[id].triNorms.xyz;
		o.customUV = _triBuffer[id].triUVs;
#endif
	}

	void setup()
	{
		unity_ObjectToWorld = _LocalToWorld;
		unity_WorldToObject = _WorldToLocal;
	}

	void surf(Input IN, inout SurfaceOutputStandard o) 
	{
		float3 texCol = tex2D(_MainTex, IN.customUV).rgb;
		o.Albedo = _Color.rgb * texCol;
		o.Metallic = _Metallic;
		o.Smoothness = _Smoothness;
		float3 normal = UnpackNormal(tex2D(_BumpMap, IN.customUV));// *float3(0, 0, IN.vface < 0 ? -1 : 1);
		normal.z = normal.z * _BumpPower;
		o.Normal = normalize(normal);
	}

	ENDCG
	}
		FallBack "Diffuse"
}
Shader "Hidden/TerrainMapRenderer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "black" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "TerrainMapRenderers.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		float3 worldPos : TEXCOORD1;
		float4 projPos : TEXCOORD2;
	};

	uniform sampler2D _CameraDepthTexture;

	sampler2D _TerrainAlbedo;
	sampler2D _NoiseTex;
	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _Heightmap;
	float4 _Heightmap_ST;

	sampler2D _Normals;
	float4 _Normals_ST;
	float4 _Normals_TexelSize;


	float _WaterHeight;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		o.projPos = ComputeScreenPos(o.vertex);
		return o;
	}

	fixed4 FragAlbedo(v2f i) : SV_Target
	{
		return tex2D(_MainTex, i.uv);
	}

	fixed4 FragHeight(v2f i) : SV_Target
	{
		float z = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;

	return float4(z, z, z, 1);
	}

	fixed4 FragNormals(v2f i) : SV_Target
	{
		return NormalFromHeight(_Heightmap, i.uv);
	}

	fixed4 FragAO(v2f i) : SV_Target
	{
		return AOFromNormal(_NoiseTex, _Heightmap, _Normals, i.uv);
	}

	fixed4 FragCurvature(v2f i) : SV_Target
	{
		return CurvatureFromNormal(_Normals, i.uv);
	}

	fixed4 FragAspect(v2f i) : SV_Target
	{
		return Aspect(_Normals, i.uv);
	}

	fixed4 FragContour(v2f i) : SV_Target
	{
		return Contours(_Heightmap, i.uv);
	}

	fixed4 FragFlow(v2f i) : SV_Target
	{
		return Flowmap(_Normals, i.uv);
	}

	fixed4 FragShoreline(v2f i) : SV_Target
	{
		float h = ClippedHeight(_Heightmap, _WaterHeight, i.uv);

		return float4(h, h, h, 1);
	}

	fixed4 FragSlope(v2f i) : SV_Target
	{
		float s = GetSlope(_Heightmap, i.uv);

		return float4(s, s, s, 1);
	}

	ENDCG

	SubShader
	{
		Pass //0
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment FragAlbedo
			ENDCG
		}

		Pass //1
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragHeight
			ENDCG
		}

		Pass //2
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragNormals
			ENDCG
		}
		Pass //3
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragAO
			ENDCG
		}
		Pass //4
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragCurvature
			ENDCG
		}
		Pass //5
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragContour
			ENDCG
		}
		Pass //6
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragFlow
			ENDCG
		}
		Pass //7
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragAspect
			ENDCG
		}
		Pass //8
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragShoreline
			ENDCG
		}
		Pass //9
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment FragSlope
			ENDCG
		}
	}
	
}

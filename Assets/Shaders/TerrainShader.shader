Shader "Unlit/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Increment ("Increment", Range(0, 1)) = 0.1
        _WaterLevel ("Water Level", Range(0, 1)) = 0.1
        _NonLineColor ("Non Line Color", Color) = (1, 1, 1, 0)
        _MinorLineColor ("Minor Line Color", Color) = (0.67, 0.67, 0.67, 1)
        _MajorLineColor ("Major Line Color", Color) = (0, 0, 0, 1)
        _ForestColor ("Forest Color", Color) = (0, 0.5, 0, 1)
        _WaterColor ("Water Color", Color) = (0, 0.5, 0, 1)
        _LineGroupAmount ("Line Group Amount", Int) = 5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _Increment;
            float _WaterLevel;
            float4 _NonLineColor;
            float4 _MinorLineColor;
            float4 _MajorLineColor;
            float4 _ForestColor;
            float4 _WaterColor;
            int _LineGroupAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
			}

            float3 getPixel(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            float roundRed(float3 color)
            {
                return round(color.r / _Increment) * _Increment;
			}

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 uv = i.uv;
                float w = 1 / _MainTex_TexelSize.z;
                float h = 1 / _MainTex_TexelSize.w;
                fixed4 color = _NonLineColor;

                // Find neighbors (Edge detection 1/3)
                float tl = roundRed(getPixel(uv + fixed2(-w, -h)));	// Top Left
                float tc = roundRed(getPixel(uv + fixed2(0, -h)));	// Top Center
                float tr = roundRed(getPixel(uv + fixed2(+w, -h)));	// Top Right
                float cl = roundRed(getPixel(uv + fixed2(-w, 0)));	// Center Left
                float cc = roundRed(getPixel(uv));					// Center Center
                float cr = roundRed(getPixel(uv + fixed2(+w, 0)));	// Center Right
                float bl = roundRed(getPixel(uv + fixed2(-w, +h)));	// Bottom Left
                float bc = roundRed(getPixel(uv + fixed2(0, +h)));	// Bottom Center
                float br = roundRed(getPixel(uv + fixed2(+w, +h)));	// Bottom Right

                // Change color of pixel to background if surrounded by similar pixels (Edge detection 2/3)
                if(tl <= cc && tc <= cc && tr <= cc && cl <= cc && cr <= cc && bl <= cc && bc <= cc && br <= cc)
                {
                    if (cc < _WaterLevel)
                        color = _WaterColor;
                    else if (tex2D(_MainTex, uv).g > 0)
                        color = _ForestColor;
				}
                // Change color of lines (Edge detection 3/3)
                else
                {
                    if (round(cc / _Increment) % _LineGroupAmount == 0)
                        color = _MajorLineColor;
                    else
                        color = _MinorLineColor;
				}

                return color;
            }
            ENDCG
        }
    }
}

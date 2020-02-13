Shader "Unlit/ContourMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Increment ("Increment", Range(0, 1)) = 0.1
        _NonLineColor ("Non Line Color", Color) = (1, 1, 1, 0)
        _LineColor ("Line Color", Color) = (0.67, 0.67, 0.67, 1)
        _LineGroupColor ("Line Group Color", Color) = (0, 0, 0, 1)
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
            float4 _LineColor;
            float4 _LineGroupColor;
            int _LineGroupAmount;
            float4 _BackgroundColor;

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

            float3 roundColor(float3 color)
            {
                return round(color / _Increment) * _Increment;
			}

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 uv = i.uv;
                float w = 1 / _MainTex_TexelSize.z;
                float h = 1 / _MainTex_TexelSize.w;
                fixed4 color = _LineColor;

                // Find neighbors (Edge detection 1/3)
                float3 tl = roundColor(getPixel(uv + fixed2(-w, -h)));	// Top Left
                float3 tc = roundColor(getPixel(uv + fixed2(0, -h)));	// Top Center
                float3 tr = roundColor(getPixel(uv + fixed2(+w, -h)));	// Top Right
                float3 cl = roundColor(getPixel(uv + fixed2(-w, 0)));	// Center Left
                float3 cc = roundColor(getPixel(uv));					// Center Center
                float3 cr = roundColor(getPixel(uv + fixed2(+w, 0)));	// Center Right
                float3 bl = roundColor(getPixel(uv + fixed2(-w, +h)));	// Bottom Left
                float3 bc = roundColor(getPixel(uv + fixed2(0, +h)));	// Bottom Center
                float3 br = roundColor(getPixel(uv + fixed2(+w, +h)));	// Bottom Right

                

                // Change color of pixel to background if surrounded by similar pixels (Edge detection 2/3)
                if(all(tl.rgb <= cc.rgb) && all(tc.rgb <= cc.rgb) && all(tr.rgb <= cc.rgb) && all(cl.rgb <= cc.rgb) && all(cr.rgb <= cc.rgb) && all(bl.rgb <= cc.rgb) && all(bc.rgb <= cc.rgb) && all(br.rgb <= cc.rgb))
                {
                    color = _BackgroundColor;
				}
                // Change color of grouped lines (Edge detection 3/3)
                else if ((cc.r / _Increment) % _LineGroupAmount == 0)
                {
                    color = _LineGroupColor;
				}

                return color;
            }
            ENDCG
        }
    }
}

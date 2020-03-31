Shader "Unlit/TerrainShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Increment ("Increment", Range(0, 0.5)) = 0.1
        _WaterLevel ("Water Level", Range(0, 1)) = 0.1
        _GroundColor ("Ground Color", Color) = (1, 1, 1, 0)
        _ForestColor ("Forest Color", Color) = (0, 0.5, 0, 1)
        _WaterColor ("Water Color", Color) = (0, 0.5, 0, 1)
        _MinorLineGroundColor ("Minor Line Ground Color", Color) = (0.67, 0.67, 0.67, 1)
        _MinorLineForestColor ("Minor Line Forest Color", Color) = (0.67, 0.67, 0.67, 1)
        _MinorLineWaterColor ("Minor Line Water Color", Color) = (0.67, 0.67, 0.67, 1)
        _MajorLineGroundColor ("Major Line Ground Color", Color) = (0, 0, 0, 1)
        _MajorLineForestColor ("Major Line Forest Color", Color) = (0, 0, 0, 1)
        _MajorLineWaterColor ("Major Line Water Color", Color) = (0, 0, 0, 1)
        _MajorLineModulo ("Major Line Modulo", Int) = 5
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
            float4 _GroundColor;
            float4 _ForestColor;
            float4 _WaterColor;
            float4 _MinorLineGroundColor;
            float4 _MinorLineForestColor;
            float4 _MinorLineWaterColor;
            float4 _MajorLineGroundColor;
            float4 _MajorLineForestColor;
            float4 _MajorLineWaterColor;
            int _MajorLineModulo;

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

            float getHeight(float3 color)
            {
                return floor((color.r - _WaterLevel) / _Increment) * _Increment;
			}

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 uv = i.uv;
                float w = 1 / _MainTex_TexelSize.z;
                float h = 1 / _MainTex_TexelSize.w;
                bool isWater = false;
                bool isForest = false;
                bool isLine = false;
                bool isMajorLine = false;
                fixed4 color = _GroundColor;
                int colorPicker = 0;

                // Find heights of neighbors
                float tl = getHeight(getPixel(uv + fixed2(-w, -h)));    // Top Left
                float tc = getHeight(getPixel(uv + fixed2(0, -h)));	    // Top Center
                float tr = getHeight(getPixel(uv + fixed2(+w, -h)));    // Top Right
                float cl = getHeight(getPixel(uv + fixed2(-w, 0)));	    // Center Left
                float cc = getHeight(getPixel(uv));					    // Center Center
                float cr = getHeight(getPixel(uv + fixed2(+w, 0)));	    // Center Right
                float bl = getHeight(getPixel(uv + fixed2(-w, +h)));    // Bottom Left
                float bc = getHeight(getPixel(uv + fixed2(0, +h)));	    // Bottom Center
                float br = getHeight(getPixel(uv + fixed2(+w, +h)));    // Bottom Right


                // Check if pixel is a major or minor line by checking if surrounded by similar pixels
                if(tl > cc || tc > cc || tr > cc || cl > cc || cr > cc || bl > cc || bc > cc || br > cc)
                {
                    isLine = true;
                    if ((floor((tex2D(_MainTex, uv).r - _WaterLevel) / _Increment) + 1) % _MajorLineModulo == 0)
                        isMajorLine = true;
				}
                
                // Check if water or forest
                if (tex2D(_MainTex, uv).r < _WaterLevel)
                    isWater = true;
                else if (tex2D(_MainTex, uv).g > 0)
                    isForest = true;

                // Calculate colorpicker based on bools
                colorPicker += isWater ? 1 : 0;
                colorPicker += isForest ? 2 : 0;
                colorPicker += isLine ? 4 : 0;
                colorPicker += isMajorLine ? 8 : 0;

                switch (colorPicker)
                {
                    case 0:
			            color = _GroundColor;
		                break;
		            case 1:
                    case 3:
			            color = _WaterColor;
		                break;
		            case 2:
			            color = _ForestColor;
		                break;
                    case 4:
                        color = _MinorLineGroundColor;
                        break;
                    case 5:
                    case 7:
                        color = _MinorLineWaterColor;
                        break;
                    case 6:
                        color = _MinorLineForestColor;
                        break;
                    case 12:
                        color = _MajorLineGroundColor;
                        break;
                    case 13:
                    case 15:
                        color = _MajorLineWaterColor;
                        break;
                    case 14:
                        color = _MajorLineForestColor;
                        break;
				}

                return color;
            }
            ENDCG
        }
    }
}

Shader "Unlit/ContourMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Increment ("Increment", Range(0, 1)) = 0.1
        _LineColor ("Line Color", Color) = (0, 0, 0, 1)
        _BackgroundColor ("Background Color", Color) = (1, 1, 1, 0)
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
            float4 _BackgroundColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float getPixel(float2 uv) {
                return tex2D(_MainText, uv.x, uv.y); //TODO: write tooling for better options
            }

            //TODO: Introduce system to grab more neighbors than 1 by adding a percentage loss variable when going to the next neighbor (and grabbing as many neighbors as necessary).

            fixed4 frag(v2f i) : SV_Target
            {
                // Get texture
                fixed2 uv = i.uv;
                float w = 1 / _MainTex_TexelSize.z;
                float h = 1 / _MainTex_TexelSize.w;
                fixed4 color = _LineColor;

                // Find neighbors (Edge detection)
                float3 tl = tex2D(_MainTex, uv + fixed2(-w, -h));	// Top Left
                float3 tc = tex2D(_MainTex, uv + fixed2(0, -h));	// Top Centre
                float3 tr = tex2D(_MainTex, uv + fixed2(+w, -h));	// Top Right
                float3 cl = tex2D(_MainTex, uv + fixed2(-w, 0));	// Centre Left
                float3 cc = tex2D(_MainTex, uv);					// Centre Centre
                float3 cr = tex2D(_MainTex, uv + fixed2(+w, 0));	// Centre Right
                float3 bl = tex2D(_MainTex, uv + fixed2(-w, +h));	// Bottom Left
                float3 bc = tex2D(_MainTex, uv + fixed2(0, +h));	// Bottom Centre
                float3 br = tex2D(_MainTex, uv + fixed2(+w, +h));	// Bottom Right

                // Group colors for all neighbors (Edge detection)
                tl = round(tl / _Increment) * _Increment;
                tc = round(tc / _Increment) * _Increment;
                tr = round(tr / _Increment) * _Increment;
                cl = round(cl / _Increment) * _Increment;
                cc = round(cc / _Increment) * _Increment;
                cr = round(cr / _Increment) * _Increment;
                bl = round(bl / _Increment) * _Increment;
                bc = round(bc / _Increment) * _Increment;
                br = round(br / _Increment) * _Increment; //TODO: do this with a list and a for-loop instead

                // Set alpha to 0 if neighbors are at the same height
                if(all(tl.rgb == cc.rgb) && all(tc.rgb == cc.rgb) && all(tr.rgb == cc.rgb) && all(cl.rgb == cc.rgb) && all(cr.rgb == cc.rgb) && all(bl.rgb == cc.rgb) && all(bc.rgb == cc.rgb) && all(br.rgb == cc.rgb))
                    color.a = 0;

                // Find neighbors (Blur)
                tl = tex2D(_MainTex, uv + fixed2(-w, -h));	// Top Left
                tc = tex2D(_MainTex, uv + fixed2(0, -h));	// Top Centre
                tr = tex2D(_MainTex, uv + fixed2(+w, -h));	// Top Right
                cl = tex2D(_MainTex, uv + fixed2(-w, 0));	// Centre Left
                cc = tex2D(_MainTex, uv);					// Centre Centre
                cr = tex2D(_MainTex, uv + fixed2(+w, 0));	// Centre Right
                bl = tex2D(_MainTex, uv + fixed2(-w, +h));	// Bottom Left
                bc = tex2D(_MainTex, uv + fixed2(0, +h));	// Bottom Centre
                br = tex2D(_MainTex, uv + fixed2(+w, +h));	// Bottom Right

                //Combine neighbors (Blur)
                float3 result =
                    tl + tc + tr +
                    cl + cc + cr +
                    bl + bc + br;

                // Either move blur to a separate pass or introduce a cutoff range (blur immediately) (DO THE LAST ONE BY WRITING MORE TOOLING)

                return color;
            }
            ENDCG
        }
    }
}

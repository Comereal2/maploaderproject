Shader "Unlit/SpriteOutlineShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0,0.1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Lighting Off
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _OutlineThickness;
                fixed4 col = tex2D(_MainTex, i.uv);

                bool nearEdge = (i.uv.x < t) || (i.uv.x > 1.0 - t) || (i.uv.y < t) || (i.uv.y > 1.0 - t);
    
                if (nearEdge)
                {
                    // Determine position along edge to apply dashing
                    float dashFreq = 20.0; // number of dashes
                    float dashThickness = 0.5; // 0 to 1, thickness of each dash

                    float pos;
                    if (i.uv.y < t || i.uv.y > 1.0 - t) {
                        // top or bottom edge: horizontal dash pattern
                        pos = i.uv.x;
                    } else {
                        // left or right edge: vertical dash pattern
                        pos = i.uv.y;
                    }

                    // Create dashed pattern using step and frac
                    float dash = step(dashThickness, frac(pos * dashFreq));

                    // Invert to keep only "on" segments
                    if (dash < 0.5) {
                        return _OutlineColor;
                    }
                }

                return col;
            }
            ENDCG
        }
    }
}

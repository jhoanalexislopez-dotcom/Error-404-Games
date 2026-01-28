Shader "Retro/PS1Water_SilentHill"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}

        _Color ("Tint Color", Color) = (0.15, 0.25, 0.3, 1)

        _TexScale ("Texture Scale", Vector) = (1,1,0,0)
        _ScrollDir ("Scroll Direction (XY)", Vector) = (0.02, 0.01, 0, 0)
        _ScrollSpeed ("Scroll Speed", Range(0,2)) = 0.3

        _WaveStrength ("Wave Strength", Range(0, 0.3)) = 0.08
        _WaveSpeed ("Wave Speed", Range(0, 3)) = 0.6

        _VertexSnap ("Vertex Snap", Range(8, 256)) = 48
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;

            float2 _TexScale;
            float2 _ScrollDir;
            float _ScrollSpeed;

            float _WaveStrength;
            float _WaveSpeed;
            float _VertexSnap;
            float _SpecularStrength;

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
            };

            v2f vert (appdata v)
            {
                v2f o;

                // --- SLOW, UNEASY WATER WAVES ---
                float wave =
                    sin(v.vertex.x * 2.0 + _Time.y * _WaveSpeed) +
                    cos(v.vertex.z * 1.5 + _Time.y * (_WaveSpeed * 0.7));

                v.vertex.y += wave * _WaveStrength;

                // --- PS1 VERTEX PRECISION ---
                float snap = _VertexSnap;
                v.vertex.xyz = floor(v.vertex.xyz * snap) / snap;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // --- TEXTURE SCALE ---
                float2 uv = v.uv * _TexScale;

                // --- SCROLLING (SILENT HILL DRIFT) ---
                uv += _ScrollDir * _Time.y * _ScrollSpeed;

                // --- AFFINE WARP (VERY SUBTLE) ---
                float warp = sin(_Time.y * 0.4 + v.vertex.x + v.vertex.z) * 0.02;
                uv += warp;

                o.uv = uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // --- DIRTY FAKE SPECULAR ---
                float viewDot = dot(
                    normalize(_WorldSpaceCameraPos - i.worldPos),
                    float3(0,1,0)
                );

                float spec = pow(saturate(viewDot), 6) * _SpecularStrength;

                fixed3 col = tex.rgb * _Color.rgb;
                col += spec;

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}

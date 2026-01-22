Shader "Unlit/WaveformCircle"
{
    Properties
    {
        _WaveformTex ("Waveform Texture", 2D) = "white" {}
        _Radius ("Circle Radius", Range(0,1)) = 0.48
        _OutlineWidth ("Outline Width", Range(0,0.2)) = 0.01
        _WaveColor ("Wave Color", Color) = (0.6,0,0.8,1)
        _BgColor ("Background Color", Color) = (0,0,0,0.6)
        _GlowStrength ("Glow Strength", Range(0,5)) = 1.5
        _Amplitude ("Wave Amplitude", Range(0,1)) = 0.8
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }

        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _WaveformTex;
            float4 _WaveColor;
            float4 _BgColor;
            float _Radius;
            float _OutlineWidth;
            float _GlowStrength;
            float _Amplitude;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float smoothMask(float dist, float radius, float width)
            {
                return 1.0 - smoothstep(radius - width, radius + width, dist);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Circle mask computation
                float2 centered = uv - float2(0.5, 0.5);
                float dist = length(centered);
                float mask = smoothMask(dist, _Radius, _OutlineWidth);

                // Base background color
                float4 bg = _BgColor * mask;

                // Waveform sampling
                float sampleU = saturate(uv.x);
                float wf = tex2D(_WaveformTex, float2(sampleU, 0.5)).r;

                // Convert waveform (0–1) so 0.5 = silence
                float amp = abs(wf - 0.5) * _Amplitude * 2.0;

                // Vertical band shape
                float mid = 0.5;
                float dy = abs(uv.y - mid);

                float softness = 0.02;
                float verticalFill = 1.0 - smoothstep(amp - softness, amp + softness, dy);

                // Fade bars toward the edges horizontally
                float xFalloff = 1.0 - smoothstep(0.25, 0.5, abs(uv.x - 0.5));
                float waveMask = verticalFill * xFalloff * mask;

                // Wave color with glow effect
                float4 wave = _WaveColor;
                wave.a *= waveMask * _GlowStrength;

                // Composite final color
                float4 color = bg;
                color.rgb = lerp(color.rgb, wave.rgb, wave.a);
                color.a = max(color.a, wave.a);

                return color;
            }


            ENDCG
        }
    }
}

Shader "Custom/CRTEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0.75,1,0.75,1)

        [Header(Scanlines)]
        _ScanlineStrength    ("Scanline Strength",  Range(0,1))    = 0.35
        _ScanlineCount       ("Scanline Count",     Range(50,800)) = 300.0

        [Header(Sweep Band)]
        _ScrollSpeed         ("Scroll Speed",       Range(0,4))    = 0.6
        _ScrollBrightness    ("Sweep Brightness",   Range(0,3))    = 1.2
        _ScrollBandSharpness ("Sweep Sharpness",    Range(5,80))   = 25.0

        [Header(Vignette)]
        _VignetteStrength    ("Vignette Strength",  Range(0,3))    = 1.8

        [Header(Distortion)]
        _DistortStrength     ("Screen Distortion",  Range(0,0.15)) = 0.018

        [Header(Glitch)]
        _GlitchStrength      ("Glitch Strength",    Range(0,1))    = 0.25

        [Header(Phosphor)]
        _PhosphorColor       ("Phosphor Color",     Color)         = (0.2, 1, 0.3, 1)
        _BloomStrength       ("Bloom Strength",     Range(0,2))    = 0.5
        _BloomSpread         ("Bloom Spread",       Range(0.002,0.03)) = 0.008

        [Header(Effects)]
        _NoiseStrength       ("Noise Strength",     Range(0,0.5))  = 0.03
        _FlickerStrength     ("Flicker Strength",   Range(0,0.2))  = 0.025

        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",         Float) = 0
        _StencilOp        ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask",         Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "CRTEffect"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            fixed4    _TextureSampleAdd;
            float4    _ClipRect;

            float _ScanlineStrength;
            float _ScanlineCount;
            float _ScrollSpeed;
            float _ScrollBrightness;
            float _ScrollBandSharpness;
            float _DistortStrength;
            float _GlitchStrength;
            float _VignetteStrength;
            float _NoiseStrength;
            float _FlickerStrength;
            fixed3 _PhosphorColor;
            float _BloomStrength;
            float _BloomSpread;

            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 screenDistort(float2 uv, float s)
            {
                uv -= 0.5;
                uv *= 1.0 + s * dot(uv, uv) * 4.0;
                uv += 0.5;
                return uv;
            }

            float onOff(float a, float b, float c)
            {
                return step(c, sin(_Time.y + a * cos(_Time.y * b)));
            }

            float ramp(float y, float start, float end)
            {
                float inside = step(start, y) - step(end, y);
                return (1.0 - (y - start) / (end - start) * inside) * inside;
            }

            float glitchStripes(float2 uv)
            {
                float n = hash(uv * float2(0.5, 1.0) + float2(1.0, 3.0));
                n *= n;
                float band = fmod(uv.y * 4.0 + _Time.y * 0.5 + sin(_Time.y + sin(_Time.y * 0.63)), 1.0);
                return ramp(band, 0.5, 0.6) * n;
            }

            float3 phosphorGlow(float2 uv, float spread)
            {
                float3 b = 0;
                b += tex2D(_MainTex, uv + float2( spread,  0)).rgb;
                b += tex2D(_MainTex, uv + float2(-spread,  0)).rgb;
                b += tex2D(_MainTex, uv + float2( 0,  spread)).rgb;
                b += tex2D(_MainTex, uv + float2( 0, -spread)).rgb;
                float s2 = spread * 2.5;
                b += tex2D(_MainTex, uv + float2( s2,  0)).rgb * 0.35;
                b += tex2D(_MainTex, uv + float2(-s2,  0)).rgb * 0.35;
                b += tex2D(_MainTex, uv + float2( 0,  s2)).rgb * 0.35;
                b += tex2D(_MainTex, uv + float2( 0, -s2)).rgb * 0.35;
                return b / 5.4;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                uv = screenDistort(uv, _DistortStrength);

                float scanY = fmod(_Time.y / 4.0, 1.0);
                float window = 1.0 / (1.0 + 20.0 * (uv.y - scanY) * (uv.y - scanY));
                uv.x += sin(uv.y * 10.0 + _Time.y) / 220.0
                      * onOff(4.0, 4.0, 0.3) * (1.0 + cos(_Time.y * 80.0))
                      * window * _GlitchStrength;

                float vShift = 0.07 * onOff(2.0, 3.0, 0.9)
                             * (sin(_Time.y) * sin(_Time.y * 20.0)
                             + 0.5 + 0.1 * sin(_Time.y * 200.0) * cos(_Time.y))
                             * _GlitchStrength;
                uv.y = frac(uv.y + vShift);

                fixed4 color = tex2D(_MainTex, uv) + _TextureSampleAdd;
                color *= IN.color;

                color.rgb += phosphorGlow(uv, _BloomSpread) * _BloomStrength;

                float sl = sin(uv.y * _ScanlineCount * UNITY_PI) * 0.5 + 0.5;
                color.rgb *= lerp(1.0, sl * sl, _ScanlineStrength);

                float sweepPos = frac(_Time.y * _ScrollSpeed);
                float beam = exp(-abs(frac(uv.y - sweepPos + 0.5) - 0.5) * _ScrollBandSharpness);
                color.rgb *= 1.0 + beam * _ScrollBrightness;

                color.rgb += glitchStripes(uv) * _GlitchStrength * 0.25;

                float n = hash(uv + frac(_Time.yy * float2(0.13, 0.27))) * 2.0 - 1.0;
                color.rgb = saturate(color.rgb + n * _NoiseStrength);

                float vigAmt = _VignetteStrength + 0.12 * sin(_Time.y + 5.0 * cos(_Time.y * 5.0));
                float2 vig = uv - 0.5;
                color.rgb *= saturate((1.0 - vigAmt * vig.y * vig.y) * (1.0 - vigAmt * vig.x * vig.x));

                color.rgb *= 1.0 + _FlickerStrength * (hash(float2(floor(_Time.y * 12.0), 0.5)) - 0.5);

                color.rgb *= _PhosphorColor;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}

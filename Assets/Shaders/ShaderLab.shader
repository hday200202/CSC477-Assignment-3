Shader "Custom/ShaderLab"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _VertColorPal ("Vertex Color Palette", 2D) = "white" {}
     }

     SubShader
     {
        Pass
        {
            Name "ShaderLab"

            CGPROGRAM
            #pragma target 3.0
            #pragma glsl
            #pragma vertex vert
            #pragma fragment frag
            #include "UNITYCG.cginc"


            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 position : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D   _MainTex;
            sampler2D   _VertColorPal;

            v2f vert(appdata v) {
                v2f o;
                float4 texel = tex2Dlod(_VertColorPal,float4(v.uv, 0, 0));
                o.color = texel;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            };

            fixed4 frag(v2f f) : COLOR {
                fixed4 texel = tex2D(_MainTex,f.uv);
                return fixed4(f.color.r, f.color.g, f.color.b, 1.0) * texel;
            }
            ENDCG
        }
    }
}

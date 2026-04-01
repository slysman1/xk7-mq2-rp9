Shader "UI/SeparableGaussianBlur"
{
    Properties {
        _Radius ("Radius", Range(0,3)) = 1
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Cull Off

        Pass { // Horizontal
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Radius;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 frag(v2f i):SV_Target{
                float2 o = float2(_MainTex_TexelSize.x * _Radius, 0);
                // 5-tap weights (fast & good-looking)
                float w0=0.2941176, w1=0.2352941, w2=0.1176471;
                fixed4 c = tex2D(_MainTex, i.uv) * w0;
                c += tex2D(_MainTex, i.uv + o) * w1;
                c += tex2D(_MainTex, i.uv - o) * w1;
                c += tex2D(_MainTex, i.uv + o*2) * w2;
                c += tex2D(_MainTex, i.uv - o*2) * w2;
                return c;
            }
            ENDCG
        }

        Pass { // Vertical
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Radius;
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(appdata_img v){ v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=v.texcoord; return o; }
            fixed4 frag(v2f i):SV_Target{
                float2 o = float2(0, _MainTex_TexelSize.y * _Radius);
                float w0=0.2941176, w1=0.2352941, w2=0.1176471;
                fixed4 c = tex2D(_MainTex, i.uv) * w0;
                c += tex2D(_MainTex, i.uv + o) * w1;
                c += tex2D(_MainTex, i.uv - o) * w1;
                c += tex2D(_MainTex, i.uv + o*2) * w2;
                c += tex2D(_MainTex, i.uv - o*2) * w2;
                return c;
            }
            ENDCG
        }
    }
}

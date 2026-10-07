Shader "Boxer/Wave1Leather" {
    Properties {
        _Color ("Leather tint", Color) = (0.065,0.055,0.045,1)
        _MainTex ("Leather grain height", 2D) = "gray" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        fixed4 _Color;
        struct Input { float2 uv_MainTex; };
        void surf (Input IN, inout SurfaceOutputStandard o) {
            float2 uv=IN.uv_MainTex*2;
            float h=tex2D(_MainTex,uv).r;
            float hx=tex2D(_MainTex,uv+float2(_MainTex_TexelSize.x*2,0)).r;
            float hy=tex2D(_MainTex,uv+float2(0,_MainTex_TexelSize.y*2)).r;
            o.Albedo=_Color.rgb*(0.85+0.25*h);
            o.Normal=normalize(float3((h-hx)*1.5,(h-hy)*1.5,1));
            o.Metallic=0.015;
            o.Smoothness=0.32*(0.6+0.4*h);
            o.Occlusion=0.75+0.25*h;
            o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

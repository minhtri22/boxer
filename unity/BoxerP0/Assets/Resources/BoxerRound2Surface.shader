Shader "Boxer/Round2Surface"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex);
                o.normal=UnityObjectToWorldNormal(v.normal);
                o.view=WorldSpaceViewDir(v.vertex); return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 n=normalize(i.normal), v=normalize(i.view), l=normalize(float3(-0.4,0.9,-0.5));
                float diffuse=saturate(dot(n,l));
                float rim=pow(1-saturate(dot(n,v)),3)*0.10;
                float spec=pow(saturate(dot(n,normalize(l+v))),32)*0.18;
                return fixed4(_Color.rgb*(0.40+0.67*diffuse)+float3(1,0.88,0.67)*(spec+rim),1);
            }
            ENDCG
        }
    }
}

Shader "Boxer/POVArmSkin"
{
    Properties { _Color ("Skin", Color) = (.53,.30,.205,1) }
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
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 view:TEXCOORD1; float2 uv:TEXCOORD2; };
            v2f vert(appdata i)
            {
                v2f o; o.pos=UnityObjectToClipPos(i.vertex); o.normal=UnityObjectToWorldNormal(i.normal);
                o.view=WorldSpaceViewDir(i.vertex); o.uv=i.uv; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 n=normalize(i.normal), v=normalize(i.view), l=normalize(float3(-.35,.85,-.45));
                float diffuse=saturate(dot(n,l));
                float softSpec=pow(saturate(dot(n,normalize(l+v))),12)*.035;
                // Restrained skin grain, no plastic glove-like highlight.
                float grain=(frac(sin(dot(i.uv,float2(127.1,311.7)))*43758.5453)-.5)*.018;
                return fixed4(_Color.rgb*(.56+.54*diffuse+grain)+softSpec,1);
            }
            ENDCG
        }
    }
}

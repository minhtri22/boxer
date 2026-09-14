Shader "Boxer/EVBackdrop" {
 Properties {_MainTex("Distant arena",2D)="black"{}}
 SubShader {Tags {"RenderType"="Opaque"} Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(A a){V o;o.pos=UnityObjectToClipPos(a.vertex);o.uv=a.uv;return o;}
 fixed4 frag(V i):SV_Target{return tex2D(_MainTex,i.uv)*fixed4(.78,.78,.78,1);}
 ENDCG
 } }
}

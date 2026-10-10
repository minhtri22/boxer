Shader "Boxer/AudienceFlash" {
 Properties {_Intensity("Local camera flash",Range(0,0.75))=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Pass {Blend SrcAlpha One ZWrite Off Cull Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Intensity;
 struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct V{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(A a){V o;o.pos=UnityObjectToClipPos(a.vertex);o.uv=a.uv;return o;}
 fixed4 frag(V i):SV_Target{float2 p=i.uv*2-1;float glow=saturate(1-dot(p,p));glow*=glow;
 float cross=saturate(1-abs(p.x)*9)*saturate(1-abs(p.y))+saturate(1-abs(p.y)*9)*saturate(1-abs(p.x));
 return fixed4(.82,.91,1,saturate(glow*.5+cross*.45)*_Intensity);}
 ENDCG
 } }
}

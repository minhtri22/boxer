Shader "Boxer/EVSurface"
{
 Properties { _Color("Tint",Color)=(1,1,1,1) _MainTex("Albedo",2D)="white"{} _Kind("Skin leather satin canvas head",Float)=0 _Reference("Reference photo",Float)=0 _FrontOnly("Front projected",Float)=0 }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _Color,_MainTex_ST; float _Kind,_Reference,_FrontOnly;
 struct A { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; };
 struct V { float4 pos:SV_POSITION; float3 n:TEXCOORD0; float3 view:TEXCOORD1; float2 uv:TEXCOORD2; float3 world:TEXCOORD3; float3 obj:TEXCOORD4; };
 V vert(A a) { V o; o.pos=UnityObjectToClipPos(a.vertex); o.n=UnityObjectToWorldNormal(a.normal); o.view=WorldSpaceViewDir(a.vertex); o.uv=a.uv; o.world=mul(unity_ObjectToWorld,a.vertex).xyz; o.obj=a.vertex.xyz; return o; }
 fixed4 frag(V i):SV_Target {
 float3 n=normalize(i.n), v=normalize(i.view);
 float3 key=normalize(float3(-.45,.85,-.65)), fill=normalize(float3(.8,.35,.45));
 float grain=sin(i.uv.x*823)*sin(i.uv.y*917)*.025;
 fixed4 sample=tex2D(_MainTex,i.uv*_MainTex_ST.xy+_MainTex_ST.zw);
 float greenDelta=sample.g-max(sample.r,sample.b);
 float greenKey=smoothstep(.10,.32,greenDelta)*smoothstep(.38,.72,sample.g);
 float frontWeight=_FrontOnly>.5?smoothstep(-.03,.12,i.obj.z):1;
 float photoWeight=sample.a*(1-greenKey)*frontWeight;
 float3 fallback=_Color.rgb;
 if(_Kind>4.5) {
   float hair=smoothstep(.70,.87,i.uv.y);
   fallback=lerp(_Color.rgb,float3(.055,.038,.030),hair);
 }
 float3 albedo=_Reference>.5?lerp(fallback,sample.rgb,photoWeight):lerp(_Color.rgb,sample.rgb*_Color.rgb,sample.a);
 // Fine material variation has no geometric/contact displacement.
 albedo*=1+grain;
 if(_Kind>.5&&_Kind<1.5) {
   float folds=pow(abs(sin(i.uv.x*39+sin(i.uv.y*31)*1.5)),42);
   albedo*=1-folds*.12;
 }
 if(_Kind>3.5) {
   float front=pow(saturate(cos(i.uv.x*6.283185)),6);
   float center=abs(sin(i.uv.x*6.283185));
   float sternum=exp(-center*center*350)*.14;
   float pec=exp(-pow((i.uv.y-.59)*38,2))*.22;
   float absLines=(exp(-pow((i.uv.y-.25)*65,2))+exp(-pow((i.uv.y-.34)*65,2))+exp(-pow((i.uv.y-.43)*65,2)))*.13;
   albedo*=1-front*(sternum+pec+absLines*saturate(1-center*2));
 }
 float spec=pow(saturate(dot(n,normalize(key+v))),_Kind==1?42:30)*(_Kind==1?.42:.24);
 float rim=pow(1-saturate(dot(n,v)),4)*saturate(n.y+.3)*.32;
 float light=.22+.72*saturate(dot(n,key))+.16*saturate(dot(n,fill));
 float3 c=albedo*light+float3(1,.86,.64)*(spec+rim);
 return fixed4(c,1);
 }
 ENDCG
 } }
}

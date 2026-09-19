Shader "Boxer/EVText" {
 Properties { _MainTex("Font atlas",2D)="white"{} }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 struct A {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct V {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 V vert(A a){V o;o.pos=UnityObjectToClipPos(a.vertex);o.uv=a.uv;o.color=a.color;return o;}
 fixed4 frag(V i):SV_Target {return fixed4(i.color.rgb,tex2D(_MainTex,i.uv).a*i.color.a);}
 ENDCG
 } }
}

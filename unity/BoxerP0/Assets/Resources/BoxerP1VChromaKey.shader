Shader "Boxer/P1VChromaKey"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Threshold ("Green threshold", Range(0,1)) = 0.22
        _Softness ("Edge softness", Range(0.001,0.5)) = 0.18
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Threshold;
            float _Softness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 sample = tex2D(_MainTex, input.uv);
                float greenDominance = sample.g - max(sample.r, sample.b);
                float subjectAlpha = 1.0 - smoothstep(_Threshold, _Threshold + _Softness, greenDominance);
                sample.a *= subjectAlpha * _Color.a;
                sample.rgb *= _Color.rgb;
                clip(sample.a - 0.01);
                return sample;
            }
            ENDCG
        }
    }
}

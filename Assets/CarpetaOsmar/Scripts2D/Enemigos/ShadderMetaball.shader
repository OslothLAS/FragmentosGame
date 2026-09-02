Shader "Custom/FiltroAgua"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color del Agua", Color) = (0, 0.5, 1, 1)
        _Cutoff ("Umbral de Fusión", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _Cutoff;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Si la suma de las transparencias no supera el umbral, recortamos el hueco
                if (col.a < _Cutoff) discard;
                
                // Si lo superan (las gotas se unieron), aplicamos el color sólido del líquido
                return float4(_Color.rgb, 1.0);
            }
            ENDCG
        }
    }
}
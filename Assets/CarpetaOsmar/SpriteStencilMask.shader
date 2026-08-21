Shader "Custom/SpriteStencilMask"
{
Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint & Transparency", Color) = (1,1,1,0.5)
    }
    SubShader
    {
        // Tags específicos para que Unity lo trate como un Sprite estándar
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off        // Para que se vea aunque el sprite esté volteado
        Lighting Off
        ZWrite Off      // En 2D no solemos escribir en el Z-Buffer
        Blend SrcAlpha OneMinusSrcAlpha // Transparencia normal de Sprite

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;    // El color del Sprite Renderer
                float2 uv       : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color; // Mezclamos con el Sprite Renderer
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.uv) * IN.color;
                
                // ¡SÚPER IMPORTANTE PARA SPRITES!
                // Si el píxel de la imagen es transparente, lo descartamos
                // para que NO escriba el número '1' en el Stencil Buffer.
                clip(c.a - 0.01); 
                
                return c;
            }
            ENDCG
        }
    }
}
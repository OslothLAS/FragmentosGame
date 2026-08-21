Shader "Custom/StencilMask"
{
    Properties
    {
        _MainTex ("Glass Texture (RGBA)", 2D) = "white" {}
        _Color ("Glass Tint & Transparency", Color) = (1,1,1,0.5) // 0.5 alfa = 50% transparente
    }

    SubShader
    {
        // 1. Cambiamos Queue a Transparent y RenderType a Transparent
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        // 2. IMPORTANTE: Habilitamos Blending para la transparencia del vidrio
        Blend SrcAlpha OneMinusSrcAlpha
        
        // 3. IMPORTANTE: Habilitamos ZWrite para la oclusión de profundidad correcta (dibujar detrás)
        ZWrite Off
        
        // 4. Eliminamos ColorMask 0; ahora queremos ver el vidrio

        Pass
        {
            // 5. Mantenemos el Stencil: escribe un '1' en los píxeles del vidrio
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // Incluimos macros útiles de Unity

            // Variables para Properties
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // Añadimos UVs para la textura
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0; // Añadimos UVs para la textura
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // Calculamos las coordenadas de textura correctas
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Muestreamos la textura y la multiplicamos por el color tint
                half4 col = tex2D(_MainTex, i.uv) * _Color;
                return col; // Retornamos el color final del vidrio
            }
            ENDCG
        }
    }
}
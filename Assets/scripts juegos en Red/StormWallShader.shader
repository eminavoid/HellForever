Shader "Custom/StormWall"
{
    Properties
    {
        _MainColor ("Storm Color", Color) = (0.5, 0, 1, 0.5) // Morado por defecto
        _MainTex ("Noise Texture", 2D) = "white" {}
        _ScrollSpeedX ("Scroll X", Range(-5, 5)) = 0.5
        _ScrollSpeedY ("Scroll Y", Range(-5, 5)) = 0.2
        _Transparency ("Transparency", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        // Esto es clave: Cull Off hace que se vea por dentro y por fuera
        Cull Off 
        // No escribir en el Z-Buffer para evitar glitches visuales en objetos transparentes
        ZWrite Off
        // Mezclado estándar para transparencia
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;
            float _ScrollSpeedX;
            float _ScrollSpeedY;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Calculamos el movimiento de la textura usando el tiempo
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.x += _Time.y * _ScrollSpeedX;
                o.uv.y += _Time.y * _ScrollSpeedY;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Obtenemos el color de la textura (ruido)
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Lo multiplicamos por nuestro color base
                col *= _MainColor;
                
                // Ajustamos la transparencia final
                col.a *= _Transparency;
                
                return col;
            }
            ENDCG
        }
    }
}
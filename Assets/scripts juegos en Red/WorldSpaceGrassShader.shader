Shader "Custom/WorldSpaceGrassShader"
{
    Properties
    {
        _GrassColorLight ("Color Pasto Claro (A)", Color) = (0.3, 0.6, 0.1, 1)
        _GrassColorDark ("Color Pasto Oscuro (B)", Color) = (0.1, 0.4, 0.05, 1)
        
        _BlendNoise ("Ruido de Mezcla (Grande)", 2D) = "gray" {}
        
        _WorldScale ("Escala Mundial (Anti-Tiling)", Range(0.001, 0.1)) = 0.01 
        
        _BlendContrast ("Contraste de la Mezcla", Range(1, 10)) = 5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
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
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;     
            };

            fixed4 _GrassColorLight;
            fixed4 _GrassColorDark;
            sampler2D _BlendNoise;
            float4 _BlendNoise_ST;
            float _WorldScale;
            float _BlendContrast;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 worldUV = i.worldPos.xz * _WorldScale;

                fixed blendValue = tex2D(_BlendNoise, worldUV).r;

                blendValue = pow(blendValue, _BlendContrast);

                fixed4 finalColor = lerp(_GrassColorLight, _GrassColorDark, blendValue);

                return finalColor;
            }
            ENDCG
        }
    }
}
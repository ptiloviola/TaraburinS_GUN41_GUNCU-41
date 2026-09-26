Shader "TD/2D/NodeFrostFire"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1) // Стандартное свойство, чтобы не было розовых квадратов

        [Header(Outward Fire Outline)]
        _OutlineThickness ("Thickness", Range(0.0, 0.1)) = 0.02
        _WobbleSpeed ("Wobble Speed", Range(0, 20)) = 5.0
        _WobbleFreq ("Wobble Frequency", Range(0, 50)) = 15.0
        _WobbleAmount ("Wobble Amount", Range(0, 0.05)) = 0.01

        [Header(Inner Frosted Glass)]
        _BlurSpread ("Blur Amount", Range(0.0, 0.05)) = 0.01
        _InnerOpacity ("Inner Opacity", Range(0, 1)) = 0.7
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _Color;
            float _OutlineThickness, _WobbleSpeed, _WobbleFreq, _WobbleAmount;
            float _BlurSpread, _InnerOpacity;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                // Читаем цвет, переданный из C# скрипта (MaterialPropertyBlock)
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Читаем альфу оригинального спрайта
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                fixed4 tint = IN.color;

                // 1. ЕСЛИ МЫ ВНУТРИ ИКОНКИ (Альфа > 0) -> Запотевшее стекло
                if (originalAlpha > 0.05)
                {
                    float b = _BlurSpread;
                    fixed4 blur = tex2D(_MainTex, IN.texcoord + float2(b, b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, b));
                    blur *= 0.25;

                    blur.rgb = lerp(blur.rgb, tint.rgb, _InnerOpacity);
                    blur.a = originalAlpha * tint.a;
                    return blur;
                }

                // 2. ЕСЛИ МЫ СНАРУЖИ ИКОНКИ (Альфа == 0) -> Огонь / Пар
                // Искажаем координаты для эффекта шевеления
                float2 wobble = float2(
                    sin(IN.texcoord.y * _WobbleFreq + _Time.y * _WobbleSpeed),
                    cos(IN.texcoord.x * _WobbleFreq - _Time.y * _WobbleSpeed)
                ) * _WobbleAmount;

                float2 sampleUV = IN.texcoord + wobble;
                
                // Ищем соседние пиксели (8 сторон)
                float t = _OutlineThickness;
                float a1 = tex2D(_MainTex, sampleUV + float2(t, 0)).a;
                float a2 = tex2D(_MainTex, sampleUV + float2(-t, 0)).a;
                float a3 = tex2D(_MainTex, sampleUV + float2(0, t)).a;
                float a4 = tex2D(_MainTex, sampleUV + float2(0, -t)).a;
                float a5 = tex2D(_MainTex, sampleUV + float2(t, t)).a;
                float a6 = tex2D(_MainTex, sampleUV + float2(-t, -t)).a;
                float a7 = tex2D(_MainTex, sampleUV + float2(-t, t)).a;
                float a8 = tex2D(_MainTex, sampleUV + float2(t, -t)).a;

                float outlineAlpha = max(max(max(a1, a2), max(a3, a4)), max(max(a5, a6), max(a7, a8)));

                // Если рядом есть пиксель иконки — рисуем край обводки
                if (outlineAlpha > 0.05)
                {
                    fixed4 outColor = tint;
                    outColor.a = outlineAlpha * tint.a * 0.9; // Слегка угасает
                    return outColor;
                }

                // В противном случае — полная прозрачность
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}
Shader "TD/2D/SpriteFogGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _GlowColor ("Material Glow Color", Color) = (1, 1, 1, 1)
        
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.1)) = 0.015
        _BlurSpread ("Inner Blur Spread", Range(0.0, 0.03)) = 0.005
        _FogOpacity ("Fog Tint Opacity", Range(0, 1)) = 0.6
        
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.5
        _PulseMax ("Pulse Max", Range(0, 5)) = 1.2
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
            float4 _GlowColor;
            float _OutlineThickness, _BlurSpread, _FogOpacity;
            float _PulseSpeed, _PulseMin, _PulseMax;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color; // Берем цвет из LineRenderer или SpriteRenderer!
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                
                // 1. УМНАЯ ПУЛЬСАЦИЯ: Проверяем, является ли цвет HDR (значение > 1)
                float maxColor = max(max(IN.color.r, IN.color.g), IN.color.b);
                float isHDR = step(1.01, maxColor); // Вернет 1, если HDR, и 0, если обычный цвет
                
                float rawPulse = lerp(_PulseMin, _PulseMax, (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5);
                float activePulse = lerp(1.0, rawPulse, isHDR); // Пульсируем ТОЛЬКО если это пройденный HDR-узел/линия!

                // Смешиваем цвет материала и цвет из C#
                fixed4 baseTint = _GlowColor * IN.color;

                // --- ВНУТРЕННЯЯ ЧАСТЬ (ЗАПОТЕВШЕЕ СТЕКЛО) ---
                if (originalAlpha > 0.05)
                {
                    float b = _BlurSpread;
                    fixed4 blur = tex2D(_MainTex, IN.texcoord + float2(b, b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, b));
                    blur *= 0.25;

                    blur.rgb = lerp(blur.rgb, baseTint.rgb, _FogOpacity);
                    blur.a = originalAlpha * baseTint.a;
                    blur.rgb *= lerp(0.9, 1.1, activePulse); 
                    blur.rgb *= blur.a;
                    return blur;
                }
                
                // --- ВНЕШНЯЯ ЧАСТЬ (ТОЛСТЫЙ КОНТУР) ---
                float outlineAlpha = 0;
                
                for(int step = 1; step <= 3; step++) 
                {
                    float t = _OutlineThickness * (step / 3.0);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(t, 0)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(-t, 0)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(0, t)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(0, -t)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(t, t)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(-t, -t)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(-t, t)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, IN.texcoord + float2(t, -t)).a);
                }
                
                if (outlineAlpha < 0.05) return fixed4(0,0,0,0);

                fixed4 outColor = baseTint;
                outColor.rgb *= activePulse;
                outColor.a = outlineAlpha * baseTint.a;
                outColor.rgb *= outColor.a;
                
                return outColor;
            }
            ENDCG
        }
    }
}
Shader "TD/2D/SpriteFogGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _GlowColor ("Glow Color", Color) = (0, 0.8, 1, 1)
        
        _OutlineSpread ("Outline Thickness", Range(0.001, 0.1)) = 0.015
        _BlurSpread ("Inner Blur Amount", Range(0.0, 0.05)) = 0.01
        _FogOpacity ("Fog Tint Opacity", Range(0, 1)) = 0.7
        
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.5
        _PulseMax ("Pulse Max", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
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
            float _OutlineSpread;
            float _BlurSpread;
            float _FogOpacity;
            float _PulseSpeed, _PulseMin, _PulseMax;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                float pulse = lerp(_PulseMin, _PulseMax, (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5);

                // --- 1. ВНУТРЕННЯЯ ЧАСТЬ (ЗАПОТЕВШЕЕ СТЕКЛО) ---
                if (originalAlpha > 0.1)
                {
                    float b = _BlurSpread;
                    // Простой Box Blur
                    fixed4 blur = tex2D(_MainTex, IN.texcoord + float2(b, b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(b, -b)) +
                                  tex2D(_MainTex, IN.texcoord + float2(-b, b));
                    blur *= 0.25;

                    // Смешиваем размытую текстуру с неоновым цветом
                    blur.rgb = lerp(blur.rgb, _GlowColor.rgb, _FogOpacity);
                    blur.a = originalAlpha * _GlowColor.a;
                    
                    // Легкая пульсация внутри
                    blur.rgb *= lerp(0.8, 1.2, pulse); 
                    blur.rgb *= blur.a;
                    return blur;
                }

                // --- 2. ВНЕШНЯЯ ЧАСТЬ (КОНТУР) ---
                float s = _OutlineSpread;
                float expandedAlpha = 0;
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(s, 0)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(-s, 0)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(0, s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(0, -s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(s, s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(-s, -s)).a);

                float isOutline = saturate(expandedAlpha - (originalAlpha * 2.0));
                
                if (isOutline < 0.1) return fixed4(0,0,0,0);

                fixed4 outColor = _GlowColor;
                outColor.rgb *= pulse;
                outColor.a = isOutline * _GlowColor.a;
                outColor.rgb *= outColor.a;
                
                return outColor;
            }
            ENDCG
        }
    }
}
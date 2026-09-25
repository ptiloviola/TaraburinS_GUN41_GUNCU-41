Shader "TD/2D/SpriteSolidOutlineGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HDR] _GlowColor ("Glow Color", Color) = (1, 1, 0, 1)
        
        // Отступ в UV координатах (от 0 до 0.05 обычно достаточно)
        _OutlineSpread ("Outline Spread", Range(0.001, 0.1)) = 0.015
        
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.5
        _PulseMax ("Pulse Max", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _GlowColor;
            float _OutlineSpread;
            float _PulseSpeed;
            float _PulseMin;
            float _PulseMax;

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
                // Читаем альфу оригинального спрайта
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                
                // ВАЖНО: Если мы находимся НА оригинальном спрайте, мы ВООБЩЕ не рисуем свечение
                if (originalAlpha > 0.1)
                {
                    return fixed4(0, 0, 0, 0);
                }

                // Сканируем соседние пиксели с ручным отступом
                float s = _OutlineSpread;
                float expandedAlpha = 0;
                
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(s, 0)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(-s, 0)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(0, s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(0, -s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(s, s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(-s, -s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(s, -s)).a);
                expandedAlpha = max(expandedAlpha, tex2D(_MainTex, IN.texcoord + float2(-s, s)).a);

                // Если вокруг тоже пусто - пиксель прозрачный
                if (expandedAlpha < 0.1)
                {
                    return fixed4(0, 0, 0, 0);
                }
                
                // Рисуем пульсирующее свечение только там, где есть край
                float pulse = lerp(_PulseMin, _PulseMax, (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5);
                fixed4 c = _GlowColor;
                
                c.rgb *= pulse;
                c.a = _GlowColor.a;
                c.rgb *= c.a; 
                
                return c;
            }
            ENDCG
        }
    }
}
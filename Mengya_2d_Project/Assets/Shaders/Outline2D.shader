Shader "Custom/Outline2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _ThicknessPixels ("Thickness Pixels", Float) = 2
        _AlphaThreshold ("Alpha Threshold", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _ThicknessPixels;
            float _AlphaThreshold;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                int steps = (int)floor(clamp(_ThicknessPixels, 1.0, 16.0));
                float2 texel = _MainTex_TexelSize.xy;
                fixed a = tex2D(_MainTex, i.uv).a;
                if (a > _AlphaThreshold)
                {
                    return fixed4(0,0,0,0);
                }
                fixed ring = 0;
                for (int k = 1; k <= steps; k++)
                {
                    float2 step = texel * k;
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(step.x, 0)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(-step.x, 0)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(0, step.y)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(0, -step.y)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(step.x, step.y)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(step.x, -step.y)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(-step.x, step.y)).a);
                    ring = max(ring, tex2D(_MainTex, i.uv + float2(-step.x, -step.y)).a);
                }
                if (ring > _AlphaThreshold)
                {
                    return _OutlineColor;
                }
                return fixed4(0,0,0,0);
            }
            ENDCG
        }
    }
}

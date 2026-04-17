Shader "Custom/VisionCone"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0.2, 0.4, 0.3)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 worldPos : TEXCOORD0;
                float4 localPos : TEXCOORD1;
            };

            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.localPos = v.vertex;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist = length(i.localPos.xy);
                float alpha = _Color.a * (1.0 - dist * 0.1);
                alpha = max(alpha, 0.05);
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}

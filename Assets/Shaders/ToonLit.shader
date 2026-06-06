Shader "CritterQuest/ToonLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.4, 0.35, 0.5, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0.001, 0.3)) = 0.05
        _RimColor ("Rim Color", Color) = (1, 0.95, 0.8, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            fixed4 _Color;
            fixed4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSoftness;
            fixed4 _RimColor;
            float _RimPower;
            float _RimStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

                // stepped lighting
                float NdotL = dot(normal, lightDir);
                float light = smoothstep(_ShadowThreshold - _ShadowSoftness,
                                        _ShadowThreshold + _ShadowSoftness, NdotL);

                // blend between shadow and lit color
                fixed4 col = lerp(_Color * _ShadowColor, _Color * _LightColor0, light);

                // ambient
                float3 ambient = ShadeSH9(float4(normal, 1.0)) * _Color.rgb * 0.6;
                col.rgb += ambient;

                // rim light
                float rim = 1.0 - saturate(dot(i.viewDir, normal));
                rim = pow(rim, _RimPower) * _RimStrength;
                col.rgb += _RimColor.rgb * rim;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

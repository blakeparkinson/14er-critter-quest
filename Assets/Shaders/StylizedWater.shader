Shader "CritterQuest/StylizedWater"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.3, 0.6, 0.7, 0.7)
        _DeepColor ("Deep Color", Color) = (0.1, 0.25, 0.4, 0.9)
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.5
        _WaveScale ("Wave Scale", Range(0.1, 5)) = 1
        _WaveHeight ("Wave Height", Range(0, 0.5)) = 0.05
        _FoamColor ("Foam Color", Color) = (0.9, 0.95, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            fixed4 _ShallowColor, _DeepColor, _FoamColor;
            float _WaveSpeed, _WaveScale, _WaveHeight;

            v2f vert (appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float wave = sin(worldPos.x * _WaveScale + _Time.y * _WaveSpeed)
                           * cos(worldPos.z * _WaveScale * 0.7 + _Time.y * _WaveSpeed * 0.8);
                v.vertex.y += wave * _WaveHeight;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = worldPos;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, normal)), 3);
                fixed4 col = lerp(_ShallowColor, _DeepColor, fresnel);

                // specular highlight
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDir = normalize(lightDir + i.viewDir);
                float spec = pow(saturate(dot(normal, halfDir)), 64) * 0.5;
                col.rgb += spec;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

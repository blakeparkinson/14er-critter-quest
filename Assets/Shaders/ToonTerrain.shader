Shader "CritterQuest/ToonTerrain"
{
    Properties
    {
        _GrassColor ("Grass Color", Color) = (0.35, 0.6, 0.25, 1)
        _GrassShadow ("Grass Shadow", Color) = (0.2, 0.35, 0.18, 1)
        _DirtColor ("Dirt Color", Color) = (0.55, 0.4, 0.25, 1)
        _RockColor ("Rock Color", Color) = (0.5, 0.48, 0.45, 1)
        _RockShadow ("Rock Shadow", Color) = (0.32, 0.3, 0.32, 1)
        _SnowColor ("Snow Color", Color) = (0.95, 0.97, 1.0, 1)
        _SnowShadow ("Snow Shadow", Color) = (0.7, 0.75, 0.9, 1)
        _AlpineColor ("Alpine Color", Color) = (0.45, 0.52, 0.3, 1)
        _GrassLine ("Grass Line", Range(0, 1)) = 0.3
        _AlpineLine ("Alpine Line", Range(0, 1)) = 0.5
        _RockLine ("Rock Line", Range(0, 1)) = 0.65
        _SnowLine ("Snow Line", Range(0, 1)) = 0.78
        _BlendSharpness ("Blend Sharpness", Range(1, 20)) = 8
        _NoiseScale ("Noise Scale", Range(0.001, 0.1)) = 0.02
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.4
        _TerrainHeight ("Terrain Height", Float) = 300
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

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
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };

            fixed4 _GrassColor, _GrassShadow, _DirtColor;
            fixed4 _RockColor, _RockShadow, _SnowColor, _SnowShadow, _AlpineColor;
            float _GrassLine, _AlpineLine, _RockLine, _SnowLine;
            float _BlendSharpness, _NoiseScale, _ShadowThreshold, _TerrainHeight;

            // simple hash noise
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1, 0));
                float c = hash(i + float2(0, 1));
                float d = hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

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

                float height = i.worldPos.y / _TerrainHeight;
                float slope = 1.0 - normal.y; // 0 = flat, 1 = vertical

                // noise for organic blending
                float n = noise(i.worldPos.xz * _NoiseScale) * 0.08;
                height += n;

                // altitude-based color selection
                fixed4 baseColor;
                fixed4 shadowColor;

                if (height > _SnowLine)
                {
                    float t = saturate((height - _SnowLine) * _BlendSharpness);
                    baseColor = lerp(_RockColor, _SnowColor, t);
                    shadowColor = lerp(_RockShadow, _SnowShadow, t);
                }
                else if (height > _RockLine)
                {
                    float t = saturate((height - _RockLine) * _BlendSharpness);
                    baseColor = lerp(_AlpineColor, _RockColor, t);
                    shadowColor = lerp(_AlpineColor * 0.7, _RockShadow, t);
                }
                else if (height > _AlpineLine)
                {
                    float t = saturate((height - _AlpineLine) * _BlendSharpness);
                    baseColor = lerp(_GrassColor, _AlpineColor, t);
                    shadowColor = lerp(_GrassShadow, _AlpineColor * 0.7, t);
                }
                else if (height > _GrassLine)
                {
                    baseColor = _GrassColor;
                    shadowColor = _GrassShadow;
                }
                else
                {
                    float t = saturate(height / _GrassLine);
                    baseColor = lerp(_DirtColor, _GrassColor, t);
                    shadowColor = lerp(_DirtColor * 0.7, _GrassShadow, t);
                }

                // steep slopes get rock
                if (slope > 0.4)
                {
                    float rockBlend = saturate((slope - 0.4) * 3.0);
                    baseColor = lerp(baseColor, _RockColor, rockBlend);
                    shadowColor = lerp(shadowColor, _RockShadow, rockBlend);
                }

                // snow sticks less on steep slopes
                if (height > _SnowLine && slope > 0.5)
                {
                    float deSnow = saturate((slope - 0.5) * 4.0);
                    baseColor = lerp(baseColor, _RockColor, deSnow);
                }

                // toon lighting
                float NdotL = dot(normal, lightDir);
                float light = smoothstep(_ShadowThreshold - 0.05, _ShadowThreshold + 0.05, NdotL);

                fixed4 col;
                col.rgb = lerp(shadowColor.rgb, baseColor.rgb, light);

                // ambient
                col.rgb += ShadeSH9(float4(normal, 1.0)) * baseColor.rgb * 0.3;

                // slight fresnel for atmosphere
                float rim = 1.0 - saturate(dot(i.viewDir, normal));
                col.rgb += float3(0.6, 0.7, 0.85) * pow(rim, 4) * 0.15;

                col.a = 1;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

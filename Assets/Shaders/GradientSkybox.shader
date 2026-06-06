Shader "CritterQuest/GradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.25, 0.45, 0.85, 1)
        _HorizonColor ("Horizon Color", Color) = (0.75, 0.85, 0.95, 1)
        _BottomColor ("Bottom Color", Color) = (0.55, 0.65, 0.55, 1)
        _SunColor ("Sun Color", Color) = (1, 0.9, 0.7, 1)
        _SunSize ("Sun Size", Range(0.01, 0.2)) = 0.05
        _SunDir ("Sun Direction", Vector) = (0.5, 0.6, -0.6, 0)
        _HorizonSharpness ("Horizon Sharpness", Range(0.5, 10)) = 3
        _CloudColor ("Cloud Color", Color) = (1, 1, 1, 0.6)
        _CloudScale ("Cloud Scale", Range(1, 20)) = 5
        _CloudSpeed ("Cloud Speed", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldDir : TEXCOORD0;
            };

            fixed4 _TopColor, _HorizonColor, _BottomColor, _SunColor, _CloudColor;
            float _SunSize, _HorizonSharpness, _CloudScale, _CloudSpeed;
            float4 _SunDir;

            float hash2(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise2(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash2(i), hash2(i + float2(1,0)), f.x),
                    lerp(hash2(i + float2(0,1)), hash2(i + float2(1,1)), f.x),
                    f.y);
            }

            float fbm(float2 p)
            {
                float v = 0;
                v += noise2(p) * 0.5;
                v += noise2(p * 2.0) * 0.25;
                v += noise2(p * 4.0) * 0.125;
                return v;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldDir = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldDir);
                float y = dir.y;

                // gradient
                fixed4 col;
                if (y > 0)
                    col = lerp(_HorizonColor, _TopColor, pow(y, 1.0 / _HorizonSharpness));
                else
                    col = lerp(_HorizonColor, _BottomColor, pow(-y, 0.5));

                // sun
                float3 sunDir = normalize(_SunDir.xyz);
                float sunDot = dot(dir, sunDir);
                float sun = smoothstep(1.0 - _SunSize, 1.0, sunDot);
                col.rgb += _SunColor.rgb * sun * 2.0;

                // sun glow
                float glow = pow(saturate(sunDot), 8) * 0.3;
                col.rgb += _SunColor.rgb * glow;

                // clouds (only above horizon)
                if (y > 0.0)
                {
                    float2 uv = dir.xz / (y + 0.1) * _CloudScale;
                    uv += _Time.y * _CloudSpeed;
                    float cloud = fbm(uv);
                    cloud = smoothstep(0.35, 0.65, cloud);
                    float cloudFade = smoothstep(0, 0.3, y);
                    col.rgb = lerp(col.rgb, _CloudColor.rgb, cloud * _CloudColor.a * cloudFade);
                }

                return col;
            }
            ENDCG
        }
    }
}

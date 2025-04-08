Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Alpha ("Alpha", float) = 0.02
        _StepSize ("Step Size", float) = 0.01
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Maximum number of raymarching samples
            #define MAX_STEP_COUNT 128

            // Allowed floating point inaccuracy
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 pos : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 oPos : TEXCOORD0;
                float3 vPos : TEXCOORD1;
            };

            struct ray
            {
                float3 dir;
                float3 origin;
            };

            struct AABB
            {
                float3 Min;
                float3 Max;
            };

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float _StepSize;
            float _Alpha;

            //find intersection points of a ray with a box - taken from https://github.com/Barbelot/Physarum3D
			bool intersectBox(ray r, AABB aabb, out float t0, out float t1)
			{
			    float3 invR = 1.0 / r.dir;
			    float3 tbot = invR * (aabb.Min-r.origin);
			    float3 ttop = invR * (aabb.Max-r.origin);
			    float3 tmin = min(ttop, tbot);
			    float3 tmax = max(ttop, tbot);
			    float2 t = max(tmin.xx, tmin.yz);
			    t0 = max(t.x, t.y);
			    t = min(tmax.xx, tmax.yz);
			    t1 = min(t.x, t.y);
			    return t0 <= t1;
			}

            float3 estimateNormal(float3 pos) {
                float3 gradient = float3(0.0, 0.0, 0.0);
                
                float3 offsets[6] = { 
                    float3(_StepSize, 0, 0), float3(-_StepSize, 0, 0),
                    float3(0, _StepSize, 0), float3(0, -_StepSize, 0),
                    float3(0, 0, _StepSize), float3(0, 0, -_StepSize)
                };

                for (int i = 0; i < 6; i++) {
                    float4 neighbor = tex3D(_MainTex, pos + offsets[i] + 0.5);
                    if (neighbor.a > 0.01) {
                        gradient += offsets[i] * 0.5; 
                    }
                }
                
                return normalize(gradient); 
             }

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            v2f vert (appdata v)
            {
                v2f o;

                // Vertex in object space. This is the starting point for the raymarching.
                o.oPos = v.pos;

                // Calculate vector from camera to vertex in world space
                float3 worldVertex = mul(unity_ObjectToWorld, v.pos).xyz;
                o.vPos = worldVertex - _WorldSpaceCameraPos;

                o.pos = UnityObjectToClipPos(v.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ray ray;
                ray.dir = mul(unity_WorldToObject, float4(normalize(i.vPos), 1));
                ray.origin = i.oPos;

                float3 samplePosition = ray.origin;
                
                float4 finalColor = (0,0,0,0);
                for (int i = 0; i < MAX_STEP_COUNT; i++)
                {
                    // Accumulate color only within unit cube bounds
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        float sampledColor = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f).r);
                        finalColor.r += sampledColor;
                        finalColor.a += sampledColor; 
                        samplePosition += ray.dir * _StepSize;
                    }
                }

                return finalColor;
            }
            ENDCG
        }
    }
}

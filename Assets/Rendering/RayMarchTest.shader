Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Alpha ("Alpha", float) = 0.02
        _StepSize ("Step Size", float) = 0.01
        _ColorOne ("Color One", Vector) = (0.7, 1.0, 0.7, 1)
        _ColorTwo ("Color Two", Vector) = (0.7, 0.9, 1.0, 1)
    }
    SubShader
    {
        Cull Off
        Tags { "Queue" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Maximum number of raymarching samples
            #define MAX_STEP_COUNT 200

            // Allowed floating point inaccuracy
            #define EPSILON 0.000001f

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
            float4 _ColorOne;
            float4 _ColorTwo;

            //find intersection points of a ray with a box - taken from GPUGems
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
            
            // Map distance to color gradient
            float3 DistanceToColor(float distance, float minDistance, float maxDistance)
            {
			    float t = saturate((distance - minDistance) / (maxDistance - minDistance));
                return lerp(_ColorOne, _ColorTwo, t);
            }

            v2f vert (appdata v)
            {
                v2f o;

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
                ray.origin = _WorldSpaceCameraPos;
                ray.dir = normalize(mul((float3x3)unity_WorldToObject, i.vPos));
                
                AABB box;
                box.Min = float3(-0.51, -0.51, -0.51);
                box.Max = float3( 0.51,  0.51,  0.51);

                float t0;
                float t1;

                intersectBox(ray, box, t0, t1);

                if(t0 < 0.0)
                {
                    t0 = 0.0;
                }
                
                float4 finalColor = float4(0,0,0,0);

                float3 rayStart = ray.origin + ray.dir * t0;
                float3 rayStop = ray.origin + ray.dir * t1;

                rayStart += .5;
                rayStop += .5;

                float dist = distance(rayStop, rayStart);
                float stepSize = dist/float(MAX_STEP_COUNT);
                float3 ds = normalize(rayStop - rayStart) * stepSize;
                
                float3 samplePosition = rayStart;
                for (int i = 0; i < MAX_STEP_COUNT; i++)
                {
                    float sampledColor = tex3D(_MainTex, saturate(samplePosition));
                    float3 color = DistanceToColor(i * stepSize, -.5, .5);
                    color *= sampledColor;
                    finalColor.rgb += color;
                    finalColor.a += sampledColor; 
                    samplePosition += ds;
                }
                

                return float4(finalColor.rgb, saturate(finalColor.a));
            }
            ENDCG
        }
    }
}

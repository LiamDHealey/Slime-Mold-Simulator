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

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float _StepSize;

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
                // Start raymarching at the front surface of the object
                float3 rayOrigin = i.oPos;

                // Use vector from camera to object surface to get ray direction
                float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vPos), 1));

                float4 color = float4(0, 0, 0, 0);
                float3 samplePosition = rayOrigin;

                for (int i = 0; i < MAX_STEP_COUNT; i++)
                {
                    if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                    {
                        float4 sampledColor = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f));

                        if(sampledColor.a > 0.1f)
                        {
                            color = sampledColor;
                            break;
                        }

                        samplePosition += rayDirection * _StepSize;
                    }
                }

                return color;
            }
            ENDCG
        }
    }
}

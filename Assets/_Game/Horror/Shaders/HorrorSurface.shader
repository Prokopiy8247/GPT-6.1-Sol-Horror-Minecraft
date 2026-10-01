Shader "MCR/HorrorSurface"
{
    Properties { _BaseColor("Pigment",Color)=(0.2,0.25,0.25,1) _Glow("Resonance",Float)=0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/_Game/Shaders/MCRCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Glow;
            CBUFFER_END
            struct Attributes {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
            struct Varyings {float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float2 uv:TEXCOORD2;};
            Varyings vert(Attributes a){Varyings v;v.world=TransformObjectToWorld(a.positionOS.xyz);v.positionCS=TransformWorldToHClip(v.world);v.normal=TransformObjectToWorldNormal(a.normalOS);v.uv=a.uv;return v;}
            half4 frag(Varyings v):SV_Target
            {
                float2 grain=floor(v.uv*48);
                float random=frac(sin(dot(grain,float2(12.9898,78.233)))*43758.5453);
                float seam=step(0.88,frac(v.uv.y*19))*0.16;
                half shade=0.6+0.4*saturate(dot(normalize(v.normal),normalize(float3(-0.4,0.8,0.3))));
                half ambient=max(0.32,_MC_Light.x*0.82);
                half3 color=MC_ToGamma(_BaseColor.rgb)*(0.84+random*0.25-seam)*shade*ambient;
                color+=MC_ToGamma(_BaseColor.rgb)*_Glow;
                float dist=length(v.world.xz-_WorldSpaceCameraPos.xz);
                float fog=saturate((dist-_MC_FogParams.x)/max(1,_MC_FogParams.y-_MC_FogParams.x));
                color=lerp(color,MC_ToGamma(_MC_FogColor.rgb),fog*_MC_FogParams.w);
                return half4(MC_ToLinear(color),1);
            }
            ENDHLSL
        }
    }
}

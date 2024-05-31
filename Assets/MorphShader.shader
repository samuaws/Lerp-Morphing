Shader"Universal Render Pipeline/MorphShader"
{
    Properties
    {
        _Slider ("Slider", Range(0,1)) = 0.0       // Slider to control morphing
        _MainTex1 ("Old Texture", 2D) = "white" {} // Texture for the first mesh
        _MainTex2 ("New Texture", 2D) = "white" {} // Texture for the second mesh
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
#define MAX_VERTICES 60024

struct appdata
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
};

struct v2f
{
    float4 pos : SV_POSITION;
    float3 normal : NORMAL;
    float2 uv : TEXCOORD0;
};

uniform float _Slider;

uniform sampler2D _MainTex1;
uniform sampler2D _MainTex2;

uniform float3 oldNearestVertices1[MAX_VERTICES];
uniform float3 newNearestVertices1[MAX_VERTICES];
uniform float3 oldNearestVertices2[MAX_VERTICES];
uniform float3 newNearestVertices2[MAX_VERTICES];

v2f vert(appdata v)
{
    v2f o;
                
    float3 oldVertex;
    float3 newVertex;

    if (_Slider < 0.5)
    {
        oldVertex = oldNearestVertices1[UNITY_VERTEX_INPUT_INSTANCE_ID];
        newVertex = newNearestVertices1[UNITY_VERTEX_INPUT_INSTANCE_ID];
    }
    else
    {
        oldVertex = oldNearestVertices2[UNITY_VERTEX_INPUT_INSTANCE_ID];
        newVertex = newNearestVertices2[UNITY_VERTEX_INPUT_INSTANCE_ID];
    }

    float3 interpolatedVertex = lerp(oldVertex, newVertex, _Slider < 0.5 ? _Slider * 2.0 : (_Slider - 0.5) * 2.0);
                
    o.pos = UnityObjectToClipPos(float4(interpolatedVertex, 1.0));
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.uv = v.texcoord;
    return o;
}

half4 frag(v2f i) : SV_Target
{
    half4 color1 = tex2D(_MainTex1, i.uv);
    half4 color2 = tex2D(_MainTex2, i.uv);
    return lerp(color1, color2, _Slider);
}
            ENDHLSL
        }
    }
}

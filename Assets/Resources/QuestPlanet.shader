Shader "BB8/Quest Planet"
{
 Properties { _Land ("Continents", 2D) = "black" {} _Earth ("Earth", Float) = 0 }
 SubShader {
  Tags { "RenderType"="Opaque" }
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 vertex:SV_POSITION; float3 surface:TEXCOORD0; float3 normal:TEXCOORD1; float3 world:TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _Land; float _Earth; float3 _SunDirection;
   v2f vert(appdata v) {
    v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.vertex=UnityObjectToClipPos(v.vertex); o.surface=v.normal;
    o.normal=UnityObjectToWorldNormal(v.normal); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o;
   }
   float hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
   float noise(float3 p) {
    float3 cell=floor(p),f=frac(p); f=f*f*(3-2*f);
    return lerp(lerp(lerp(hash(cell),hash(cell+float3(1,0,0)),f.x),lerp(hash(cell+float3(0,1,0)),hash(cell+float3(1,1,0)),f.x),f.y),
     lerp(lerp(hash(cell+float3(0,0,1)),hash(cell+float3(1,0,1)),f.x),lerp(hash(cell+float3(0,1,1)),hash(cell+float3(1,1,1)),f.x),f.y),f.z);
   }
   float4 frag(v2f i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float3 s=normalize(i.surface),n=normalize(i.normal);
    float terrain=noise(s*4+8)*.65+noise(s*12+19)*.35;
    float3 color=lerp(float3(.38,.19,.075),float3(.9,.66,.34),terrain);
    if(_Earth>.5) {
     float latitude=asin(s.y);
     float2 uv=float2(frac(atan2(s.x,s.z)/UNITY_TWO_PI+.5),latitude/UNITY_PI+.5);
     float land=tex2D(_Land,uv).r;
     float dryness=saturate(1-abs(latitude*57.2958-23)/16);
     float3 soil=lerp(float3(.16,.38,.15),float3(.72,.6,.35),dryness);
     color=lerp(float3(.025,.19,.5),soil*(.8+terrain*.4),land);
     color=lerp(color,float3(.9,.94,1),saturate((noise(s*9+23)-.6)*2.4));
     color=lerp(color,.94,smoothstep(.96,.995,abs(s.y))*.85);
    }
    color*=.36+.8*max(0,dot(n,normalize(_SunDirection)));
    float rim=pow(1-saturate(dot(n,normalize(_WorldSpaceCameraPos-i.world))),4);
    color+=lerp(float3(.6,.38,.15),float3(.1,.45,.85),_Earth)*rim*.55;
    return float4(color,1);
   }
   ENDCG
  }
 }
 Fallback Off
}

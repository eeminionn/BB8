Shader "BB8/Quest Controller" {
 Properties { _MainTex("Texture",2D)="white" {} _Glow("Pressed",Color)=(0,0,0,0) }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 sampler2D _MainTex;float4 _Glow;
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target{return fixed4(tex2D(_MainTex,i.uv).rgb*(.7+.3*max(0,dot(normalize(i.normal),normalize(float3(-.3,1,-.4)))))+_Glow.rgb,1);}
 ENDCG
 }}
}

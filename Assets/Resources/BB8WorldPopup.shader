Shader "BB8/World Popup" {
 Properties { _MainTex ("Texture", 2D) = "white" {} _Color ("Color", Color) = (1,1,1,1) _Font ("Font alpha", Float) = 0 }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off
  ZTest LEqual
  Cull Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _MainTex; fixed4 _Color; float _Font;
   v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
   fixed4 frag(v2f i):SV_Target { fixed4 t=tex2D(_MainTex,i.uv); t.rgb=lerp(t.rgb,fixed3(1,1,1),_Font); return t*_Color*i.color; }
   ENDCG
  }
 }
}

Shader "BB8/MenuPlanet"
{
    Properties { _Land ("Continents", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _Land;
            float _Rotation, _Earth;

            float hash(float3 p) { return frac(sin(dot(p, float3(127.1,311.7,74.7))) * 43758.5453); }
            float noise(float3 p)
            {
                float3 cell = floor(p), f = frac(p);
                f = f*f*(3-2*f);
                return lerp(lerp(lerp(hash(cell),hash(cell+float3(1,0,0)),f.x),
                                 lerp(hash(cell+float3(0,1,0)),hash(cell+float3(1,1,0)),f.x),f.y),
                            lerp(lerp(hash(cell+float3(0,0,1)),hash(cell+float3(1,0,1)),f.x),
                                 lerp(hash(cell+float3(0,1,1)),hash(cell+float3(1,1,1)),f.x),f.y),f.z);
            }
            float4 frag(v2f_img i) : SV_Target
            {
                float2 xy = i.uv * 2 - 1;
                float r2 = dot(xy,xy);
                if (r2 >= 1) return 0;
                float3 normal = float3(xy, sqrt(1-r2));
                // Rotate surface coordinates around north; keep the silhouette and sun fixed.
                float s = sin(_Rotation), c = cos(_Rotation);
                float3 surface = float3(c*normal.x+s*normal.z, normal.y, -s*normal.x+c*normal.z);
                float terrain = noise(surface*4+8)*.65 + noise(surface*12+19)*.35;
                float3 color = lerp(float3(.30,.16,.075), float3(.72,.53,.29),terrain);
                if (_Earth > .5)
                {
                    float latitude = asin(surface.y);
                    float2 mapUV = float2(frac(atan2(surface.x,surface.z)/UNITY_TWO_PI+.5), latitude/UNITY_PI+.5);
                    float land = tex2D(_Land,mapUV).r;
                    float dryness = saturate(1-abs(latitude*57.2958-23)/16);
                    float3 soil = lerp(float3(.16,.32,.14),float3(.65,.52,.30),dryness);
                    color = lerp(float3(.035,.16,.33),soil*(.8+terrain*.4),land);
                    float clouds = saturate((noise(surface*9+23)-.64)*2);
                    color = lerp(color,float3(.83,.86,.84),clouds);
                    color = lerp(color, .9, smoothstep(.96,.995,abs(surface.y))*.75);
                }
                color *= .13+.87*max(0,dot(normal,normalize(float3(-.6,.55,.7))));
                float3 atmosphere = lerp(float3(.6,.38,.15),float3(.12,.42,.7),_Earth);
                color += atmosphere*pow(1-normal.z,4)*.6;
                return float4(color,saturate((1-r2)*170));
            }
            ENDCG
        }
    }
    Fallback Off
}

Shader "Custom/StableSpaceSkybox"
{
    Properties
    {
        _StarDensity ("Star Density", Range(0, 5)) = 1
        _BaseColor ("Base Color", Color) = (0.1, 0.1, 0.2, 1)
        _Num1 ("Num1", Range(0, 24)) = 12.9898
        _Num2 ("Num2", Range(0, 100)) = 78.233
        _Num3 ("Num3", Range(0, 100)) = 45.164
        _Num4 ("Num4", Range(0, 50000)) = 43758.5453
        _Step ("Step", Range(0, 1)) = 0.6
    }
    SubShader
    {
        Tags { "Queue"="Background" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float _StarDensity;
            fixed4 _BaseColor;
            float _Num1;
            float _Num2;
            float _Num3;
            float _Num4;
            float _Step;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = normalize(v.vertex.xyz);
                return o;
            }

            float hash(float3 p) {
                return frac(sin(dot(p, float3(_Num1,_Num2, _Num3))) * _Num4);
            }

            fixed4 frag(v2f i) : SV_Target {
                float3 d = normalize(i.dir);

                float starNoise = hash(floor(d * 500.0));
                float stars = step(1.0 - _StarDensity * 0.001, starNoise);
                float stars2 = step(1.0 - (_StarDensity / 1.5) * 0.001, starNoise);

                float t = saturate(d.y * 0.5 + 0.5); // перетворюємо [-1,1] -> [0,1]
                fixed3 skyColor = lerp(_BaseColor.rgb * 0.5, _BaseColor.rgb * 1.5, t);

                return fixed4(skyColor.r + stars + (stars2 * (1 + sin(_Time.y * 5) / 2)), skyColor.g + stars + (stars2 * (1 - sin(_Time.y * -5) / 2)), skyColor.b + (stars2 * (1 + sin(_Time.y * 5) / 2)), 1);
            }
            ENDCG
        }
    }
}

Shader "Custom/PlanetSurface"
{
    Properties
    {
        _OceanTex ("Ocean", 2D) = "white" {}
        _GrassTex ("Grass", 2D) = "white" {}
        _RockTex ("Rock", 2D) = "white" {}
        _SnowTex ("Snow", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _OceanTex, _GrassTex, _RockTex, _SnowTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float h = IN.uv_MainTex.x; // висота з UV.x

            fixed4 col;
            if (h < 0.3) col = tex2D(_OceanTex, IN.uv_MainTex * 10);
            else if (h < 0.5) col = tex2D(_GrassTex, IN.uv_MainTex * 10);
            else if (h < 0.7) col = tex2D(_RockTex, IN.uv_MainTex * 10);
            else col = tex2D(_SnowTex, IN.uv_MainTex * 10);

            o.Albedo = col.rgb;
            o.Metallic = 0;
            o.Smoothness = 0.3;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

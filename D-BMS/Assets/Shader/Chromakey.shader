Shader "Unlit/Chromakey"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Border("BorderSetting",Range(0,1000)) = 100
        _KeyColor("Key Color", Color) = (1,1,1)
        _Near("Near", Range(0, 2)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half _Border;
            fixed4 _KeyColor;
            half _Near;
            fixed2 bound(fixed2 st, float i)
            {
                fixed2 p = floor(st) + i;
                return p;
            }
                        
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // Chroma Key
                fixed4 c1 = tex2D (_MainTex, bound(i.uv * _Border, 1)/_Border);
                clip(distance(_KeyColor, c1) - _Near);
                fixed4 c2 = tex2D (_MainTex, bound(i.uv * _Border, 0)/_Border);
                clip(distance(_KeyColor, c2) - _Near);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

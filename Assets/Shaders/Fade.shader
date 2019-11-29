Shader "TNTC/Fade"{
    Properties{
        _MainColor ("Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range (0, 1)) = 1
       
    }
    SubShader{
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass{
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata{
                float4 vertex : POSITION;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
            };

			float4 _MainColor;
            float _Alpha;

            v2f vert (appdata v){
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{

                fixed4 col = _MainColor;
                col.a = _Alpha;

                return col;
            }
            
            ENDCG
        }
    }
}

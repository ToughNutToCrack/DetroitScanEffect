Shader "TNTC/ScannerEffect"{
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_ScanDistance1("Scan Distance First", float) = 0
        _ScanDistance2("Scan Distance Second", float) = 0
		_ScanWidth("Scan Width", float) = 10
        [HDR]_FirstColor("First Color", Color) = (1, 1, 1, 1)
        [HDR]_SecondColor("Second Color", Color) = (0, 0, 0, 1)
        [HDR]_FillColor("Fill Color", Color) = (0, 0, 0, 1)
	}

	SubShader{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct VertIn{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 ray : TEXCOORD1;
			};

			struct VertOut{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 depth : TEXCOORD1;
				float4 ray : TEXCOORD2;
			};

			float4 _MainTex_TexelSize;
			float4 _CameraWS;

			VertOut vert(VertIn v){
				VertOut o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy;
				o.depth = v.uv.xy;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif				

				o.ray = v.ray;

				return o;
			}

			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture;
			float4 _WorldSpaceScannerPos;
			float _ScanDistance1;
			float _ScanDistance2;
			float _ScanWidth;

			float4 _FirstColor;
			float4 _SecondColor;
			float4 _FillColor;

            float4 rectangle(float3 pos, float distanceX, float distanceZ, float width, float4 col){
                if (pos.x > - distanceX && pos.x < distanceX
                    &&
                    pos.z > - distanceZ && pos.z < distanceZ
                ){
                    if(((pos.x > - distanceX && pos.x < - distanceX + width) 
                        || 
                        (pos.x < distanceX && pos.x > distanceX - width)) 
                        ||
                        ((pos.z > - distanceZ && pos.z < - distanceZ + width) 
                        || 
                        (pos.z < distanceZ && pos.z > distanceZ - width)) 
                    ){
                        return col;
                    }
                }
                return 0;
            }

			float4 fillArea(float3 pos, float distanceX, float distanceZ, float4 col){
				if (distanceX > 0 && distanceZ > 0 && pos.x > - distanceX && pos.x < distanceX && pos.z > - distanceZ && pos.z < distanceZ){
					return col;
				}
                return 0;
			}

			half4 frag (VertOut i) : SV_Target{
				half4 col = tex2D(_MainTex, i.uv);

				float rawDepth = DecodeFloatRG(tex2D(_CameraDepthTexture, i.depth));
				float linearDepth = Linear01Depth(rawDepth);
				float4 dir = linearDepth * i.ray;
				float3 pos = _WorldSpaceCameraPos + dir;
				
				half4 scannerCol = half4(0, 0, 0, 0);

                float3 diff = pos - _WorldSpaceScannerPos;
                
                float4 r1 = rectangle (diff, _ScanDistance1, _ScanDistance2, _ScanWidth, _FirstColor);
                float4 r2 = rectangle (diff, _ScanDistance2, _ScanDistance1, _ScanWidth, _SecondColor);
                float4 fill = fillArea (diff, _ScanDistance2-1, _ScanDistance1-1, _FillColor);
				
                scannerCol = col;
				scannerCol *= lerp(1, r1, r1.w);
				scannerCol *= lerp(1, r2, r2.w);
				scannerCol *= lerp(1, fill, fill.w);

				return scannerCol;
                
			}
			ENDCG
		}
	}
}

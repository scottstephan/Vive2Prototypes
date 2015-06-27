Shader "Underwater/DiffCausDispUV3" {
    Properties
	{
      _MainTex ("Texture", 2D) = "white" {}
	  _NoiseAmp ("Noise Amplitude", float) = 6.0
	  _CausticTex ("Caustic Texture", 2D) = "black" {}
	  _alphaTest ("Alpha cutoff", Range (0,1)) = 0.5
	  _ambMult ("Ambient Mult", float) = 1
    }
    SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			pass{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			Alphatest Greater [_alphaTest]
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _CausticTex;
				float _CausticSpeed;
				float _causticsTileMult;
				float _causticsMult;
				float4x4 _lightMatrix;
				float4 _DeepColor;
				float4 _ShallowColorMinusDeep;
				float _DepthMax;
				float _ShallowPoint;
				float _DeepestPoint;
				float _DeepestPointBrightness;
				float _DepthPower;
				float _NoiseAmp;
				float4 _lightDir;
				float4 _keyLightColor;
				float _keyLightMult;
				float4 _ambLightColor;
				float _ambLightMult;
				float4 _OceanCurrents;
				float _ambMult;
			  
				struct v2f 
				{
					float4  pos : SV_POSITION;
					float4  uv : TEXCOORD0;
					float4  util : TEXCOORD1;
					float  anim : TEXCOORD2;
				};
			  
				float4 _MainTex_ST;
				float4 _CausticTex_ST;

				v2f vert (appdata_full v)
				{
					v2f o;
					float4 wp = mul(_Object2World, v.vertex);
					float4 os = wp;
					float yRamp = pow(v.texcoord1.y,3);
					os.x += _NoiseAmp * yRamp * _OceanCurrents.x;
					os.z += _NoiseAmp * yRamp * _OceanCurrents.z;
					float4 ns = mul(_World2Object, os);				
					o.pos = mul (UNITY_MATRIX_MVP, ns);
					
					o.uv.xy = TRANSFORM_TEX (v.texcoord, _MainTex);

					float2 causUV = mul (_lightMatrix, wp);
					o.uv.z = _causticsTileMult * causUV.x+ 0.02*sin(_Time*_CausticSpeed +causUV.y*40 * _causticsTileMult) + _Time ;
					o.uv.w = _causticsTileMult * causUV.y+ 0.02*sin(_Time*_CausticSpeed +causUV.x*40 * _causticsTileMult) ;
										
					float3 lightPos = mul(_World2Object,-10000 * _lightDir).xyz;
					float3 lightDirection = lightPos - v.vertex.xyz;

					float NdotL = max(dot(normalize(lightDirection),normalize(v.normal.xyz)),0.3);
					float dis = length(_WorldSpaceCameraPos - wp.xyz);
					o.util.x = pow(clamp(1-dis/_DepthMax,0,1),_DepthPower); //depth
					o.util.y = clamp(1+(wp.y+_ShallowPoint)/_DeepestPoint ,0,1); //deep
					o.util.w = NdotL;
					o.util.z = max(o.util.y,_DeepestPointBrightness);
					o.anim = clamp(1-dis/400,0,1);
					return o;
				}

				half4 frag (v2f i) : COLOR
				{
					half4 texcol = tex2D (_MainTex, i.uv.xy) ;
					half4 texcaustic = tex2D (_CausticTex, i.uv.zw).g*i.util.w;
					half4 waterColor = _DeepColor + (i.util.y * _ShallowColorMinusDeep);
					//half4 col = i.util.z * texcol*( i.util.w * i.util.x * _keyLightColor * _keyLightMult + _ambLightColor * _ambLightMult)+ texcaustic * _causticsMult*i.anim;
					half4 col = _ambMult * texcol*( i.util.w  * _keyLightColor * _keyLightMult + _ambLightColor * _ambLightMult)+ texcaustic * _causticsMult*i.anim;
					half4 outCol = waterColor*(1-i.util.x) + i.util.x * col;
					outCol.a = texcol.a;
					return outCol;
				}
				
			ENDCG
		}
    }
    Fallback "Diffuse"
  }
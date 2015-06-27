Shader "Valve/valveDiffCausRim" {
  Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Bumpmap", 2D) = "bump" {}
	_CausticTex ("Caustic Texture", 2D) = "black" {}
	_CausScale ("Caustic Scale", float) = 1
	_AmbMult ("Ambient Mult", float) = 0
	_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
	_RimMult ("Rim Mult", float) = 0.5
	_RimColor ("Rim Color", color) = (1,1,1,1)
	//_SpecPower ("Spec Power", Range(0.2,8.0)) = 1.0
	//_SpecMult ("Spec Mult", float) = 0.5
	//_SpecColor ("Spec Color", color) = (1,1,1,1)
  }
  SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200
    
    CGPROGRAM
    #pragma surface surf Lambert finalcolor:mycolor vertex:myvert
    //#pragma surface surf BlinnPhong  finalcolor:mycolor vertex:myvert

	sampler2D _MainTex;
	sampler2D _BumpMap;
	half _AmbMult;
	half _RimPower;
	half _RimMult;
	half4 _RimColor;
	//half _SpecPower;
	//half _SpecMult;

	half _CausScale;
	sampler2D _CausticTex;
	float _causticsMult;
	float _causticsTileMult;
	float _CausticSpeed;
	float4x4 _lightMatrix;


	float _DepthMax;
	float4 _DeepColor;
	float4 _ShallowColorMinusDeep;
	float _ShallowPoint;
	float _DeepestPoint;

    struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float3 viewDir;
		half fog;
		half deep;
		float4 causUV;
    };

    
    void myvert (inout appdata_full v, out Input data) {
		UNITY_INITIALIZE_OUTPUT(Input,data);
		float pos = length(mul (UNITY_MATRIX_MV, v.vertex).xyz);
		float diff = _DepthMax;
		float invDiff = 1.0f / diff;
		data.fog = clamp ((_DepthMax - pos) * invDiff, 0.0, 1.0);


		float4 wp = mul(_Object2World,v.vertex);
		data.deep = clamp(1+(wp.y+_ShallowPoint)/_DeepestPoint ,0,1); //deep

	
		
		float2 causUV = mul (_lightMatrix, mul(_Object2World, v.vertex * 0.3 / _CausScale));
		float ts1 = _Time*_CausticSpeed;
		float ts2 = ts1*1.5;
		float ctm1 = 0.004 * 40;
		float ctm2 = 0.004 * 220;
		data.causUV.x = 0.004 * causUV.x+ 0.02*sin(ts1 +causUV.y*ctm1)  ;
		data.causUV.y = 0.004 * causUV.y+ 0.02*sin(ts1 +causUV.x*ctm1) 	;
		data.causUV.z = v.tangent.w;
    }
    void mycolor (Input IN, SurfaceOutput o, inout fixed4 color) {
		half4 waterColor = _DeepColor + (IN.deep * _ShallowColorMinusDeep);
		color.rgb = lerp (waterColor, color.rgb, IN.fog);
    }

    void surf (Input IN, inout SurfaceOutput o) {
		half4 c = tex2D (_MainTex, IN.uv_MainTex);
		half texcaustic = tex2D (_CausticTex, IN.causUV.xy).g ;
		o.Normal =  UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) ;
        o.Albedo =  c.rgb + texcaustic * _causticsMult;//;//
        
        //o.Specular = _SpecPower;
        //o.Gloss = _SpecMult * c.a;
		//o.Alpha = c.a;
		half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
		o.Emission =   _RimColor.rgb * pow (rim, _RimPower) * _RimMult * c.a + _AmbMult * c;
      
    }
    ENDCG
  } 
  FallBack "Diffuse"
}
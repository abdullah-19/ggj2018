// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TENKOKU/TenkokuFog" {

Properties {
	_MainTex ("Base (RGB)", 2D) = "black" {}
	_SkyTex ("Base (RGB)", 2D) = "black" {}
	//_FogDistance("Fog Distance",Float) = 1.0
}



CGINCLUDE

	#pragma target 3.0
	#include "UnityCG.cginc"
	#include "Lighting.cginc"

	uniform sampler2D _MainTex, _SkyTex;
	uniform sampler2D_float _CameraDepthTexture;
	sampler2D _CameraDepthNormalsTexture;

	uniform float4 _HeightParams;
	
	// x = start distance
	uniform float4 _DistanceParams;
	
	int4 _SceneFogMode;
	float4 _SceneFogParams;



float _Tenkoku_FogStart;
float _Tenkoku_FogEnd;


	uniform float4 _MainTex_TexelSize;
	
	// for fast world space reconstruction
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;

	half4 _Tenkoku_FogColor;
	float _fogSkybox;
	float _fogHorizon;
	float _FogStart;
	float _FogDistance;
	float _camDistance;

	float _Tenkoku_Ambient;
	float _Tenkoku_AmbientGI;
	float _Tenkoku_AtmosphereDensity;
	float _Tenkoku_FogDensity;
	float4 _Tenkoku_overcastColor;
	float _Tenkoku_overcastAmt;
	float _tenkokufogFull;

float4 Tenkoku_Vec_SunFwd;
float4 Tenkoku_Vec_MoonFwd;
float4 Tenkoku_Vec_LightningFwd;
float4 Tenkoku_LightningColor;

float4x4 _Tenkoku_CameraMV;
samplerCUBE _Tenkoku_EnvironmentCube;
samplerCUBE _Tenkoku_SkyCube;
samplerCUBE _Tenkoku_SnowCube;
sampler2D _Tenkoku_TexFX;
sampler2D _Tenkoku_ParticleTex;


sampler2D _HeatDistortText;
float _Tenkoku_HeatDistortAmt;
float _HeatDistortSpeed;
float _HeatDistortScale;
float _HeatDistortDist;
float _tenkoku_rainbowFac1, _tenkoku_rainbowFac2;
float Tenkoku_LightningLightIntensity;
float _Tenkoku_shaderDepth;
float _Tenkoku_FadeDistance;
float _Tenkoku_UseElek;

sampler2D _Tenkoku_SkyTex;
sampler2D _Tenkoku_SkyBox;


	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
		float4 screenPos: TEXCOORD3;
	};
	
	v2f vert (appdata_img v)
	{
		v2f o;
		half index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.screenPos=ComputeScreenPos(o.pos);

		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;
		
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0){
			o.uv.y = 1-o.uv.y;
		}
		#endif				


		//set frustrum indexes specifically
		// this fixes fog/effect errors under webGL
		if (0 == (int)index)
			o.interpolatedRay = _FrustumCornersWS[0];
		else if (1 == (int)index)
			o.interpolatedRay = _FrustumCornersWS[1];
		else if (2 == (int)index)
			o.interpolatedRay = _FrustumCornersWS[2];
		else
			o.interpolatedRay = _FrustumCornersWS[3]; 
			
		//o.interpolatedRay.w = index;


		return o;
	}
	

	// Distance-based fog
	float ComputeDistance (float3 camDir, float zdepth)
	{
		float dist; 
		if (_SceneFogMode.y == 1)
			dist = length(camDir);
		else
			dist = zdepth;// * _ProjectionParams.z;

		return dist;
	}

	// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
	float ComputeHalfSpace (float3 wsDir)
	{
		float3 wpos = _CameraWS + wsDir;
		float FH = _HeightParams.x;
		float3 C = _CameraWS;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y-FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1-2*k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV+1.0e-5f));
		return g;
	}

	half4 ComputeFog (v2f i, bool distance, bool height) : SV_Target
	{



		//------ Turned off temporarily --------------
		//CALCULATE VIEW NORMALS
		//float3 normalValues;
		//float depthValue;
		//DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.screenPos.xy), depthValue, normalValues);
		//float4 viewNormalColor = float4(normalValues, 1);

		//GET WORLD NORMAL CUBEMAP
		//float4 worldNormalColor = texCUBE(_Tenkoku_EnvironmentCube,mul(_Tenkoku_CameraMV, float4(normalValues, 0)).xyz);

		//GET SKY CUBEMAP
		//float4 skyCube = texCUBE(_Tenkoku_SkyCube,mul(_Tenkoku_CameraMV, float4(normalValues, 0)).xyz);
		// --------



		// Reconstruct world space position & direction
		// towards this screen pixel.
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
		float dpth = Linear01Depth(rawDepth);
		float4 wsDir = dpth * i.interpolatedRay;


		//CALCULATE FOG
		float diff = _Tenkoku_FogEnd - _Tenkoku_FogStart;
		float invDiff = abs(diff) > 0.0001f ? 1.0 / diff : 0.0;
		_SceneFogParams.z = -invDiff;
		_SceneFogParams.w = _Tenkoku_FogEnd * invDiff;
		half usedpth = _DistanceParams.z + ComputeDistance(wsDir, dpth) + ComputeHalfSpace (wsDir);
		half fogFac = (saturate(max(0.0,usedpth) * _SceneFogParams.z + _SceneFogParams.w));



		//CALCULATE HEAT EFFECT OVERLAY
		half heatDistFac = (saturate(max(0.0,usedpth) * _SceneFogParams.z + _SceneFogParams.w));
		heatDistFac += (saturate(wsDir.y/2000));
		heatDistFac = saturate(lerp(1,0-_HeatDistortDist,heatDistFac));

		half3 distortTex = UnpackNormal(tex2D(_HeatDistortText,i.uv*_HeatDistortScale+float2(0.0,-_Time.x*_HeatDistortSpeed)));
		half2 dUV = i.uv;
		dUV.x += (distortTex.x * (_Tenkoku_HeatDistortAmt*(heatDistFac)));


		i.uv_depth = dUV;
		i.uv = dUV;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0){
			i.uv_depth.y = 1.0-i.uv_depth.y;
		}
		#endif	
		float2 uvx = i.uv_depth;

		//Recalculate Depth with Heat Distortion
		rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
		dpth = Linear01Depth(rawDepth);
		wsDir = dpth * i.interpolatedRay;


		//Get Scene Color Info
		half4 sceneColor = tex2D(_MainTex, i.uv);


		//Recalculate fog with heat distortion
		diff = _Tenkoku_FogEnd - _Tenkoku_FogStart;
		invDiff = abs(diff) > 0.0001f ? 1.0 / diff : 0.0;
		_SceneFogParams.z = -invDiff;
		_SceneFogParams.w = _Tenkoku_FogEnd * invDiff;
		usedpth = _DistanceParams.z + ComputeDistance(wsDir, dpth) + ComputeHalfSpace (wsDir);
		fogFac = (saturate(max(0.0,usedpth) * _SceneFogParams.z + _SceneFogParams.w));



		// Handle skybox fog
		if (dpth == 1.0){
			if (_fogSkybox == 1.0){
				fogFac = 1.0;
			}
			if (_fogSkybox == 0.0){
				fogFac = 0.0;
			}
		}


		fogFac = saturate(fogFac+saturate(lerp(1.0,0.0,_Tenkoku_AtmosphereDensity*2.0)));

		//Handle Horizon Fog
		float diff2 = _tenkokufogFull - (10.0);
		float invDiff2 = abs(diff2) > 0.0001f ? 1.0 / diff2 : 0.0;
		half fogFac3 = saturate(max(0.0,usedpth) * -invDiff2 + (_tenkokufogFull * invDiff2));

		if (_fogHorizon == 1.0){
			fogFac *= saturate((wsDir.y/min(_tenkokufogFull,250.0)) + fogFac3);
		}



		//read texture mipmap based on depth
		half colFac = 1;
		if (dpth >= 1.0){
			colFac = 0;
		}

		//_SkyTex
		half4 skyColor = tex2Dlod(_Tenkoku_SkyTex, float4(uvx.x,uvx.y,0,0));
		skyColor = lerp(skyColor,skyColor * _Tenkoku_FogColor,_Tenkoku_FogColor.a * (colFac));
		half4 fCol = lerp(skyColor, sceneColor, fogFac);



		//DARKEN FOG
		if (dpth < 1.0){
			fCol = fCol * lerp(0.98,0.4,saturate(lerp(0,1.0,saturate(_Tenkoku_overcastColor.a))));
		}



		fCol.rgb = lerp(fCol.rgb,fCol.rgb*0.65,saturate(_Tenkoku_overcastColor.a*4.0));



		//set overall fog density
		if (dpth < 1.0){
			fCol = lerp(sceneColor,fCol,_Tenkoku_FogDensity);

			//lightning
			half lVec = saturate(dot(Tenkoku_Vec_LightningFwd.xyz, normalize( i.interpolatedRay.xyz))) - 0.1;
			fCol.rgb = lerp(fCol.rgb, Tenkoku_LightningColor, (1-fogFac) * 2.6 * lVec * saturate(lerp(1.0,0.2,rawDepth)) * Tenkoku_LightningLightIntensity * 0.2f);

		}


		
		if (dpth >= 1.0){
			if (_Tenkoku_UseElek == 1.0){
				fCol = sceneColor;
			}
			fCol = lerp(fCol, sceneColor, saturate(lerp(-0.1,1.0, ((dot(half3(0,1,0), i.interpolatedRay.xyz)*0.0005)))));
		}





//RENDER RAINBOWS
half3 origCol = fCol.rgb;
half3 rainCol = 0;
half3 rainCol2 = 0;
half rFac = saturate((fCol.r*0.4) + (fCol.g*0.8) + (fCol.b*0.5));
half rVec = saturate(dot(Tenkoku_Vec_SunFwd.xyz, normalize( -i.interpolatedRay.xyz))) - 0.1;
half rVec2 = saturate(dot(Tenkoku_Vec_SunFwd.xyz, normalize( -i.interpolatedRay.xyz)) + 0.1);

//primary
half rS1 = 12;
rainCol.rgb = lerp(rainCol,half3(1.7,0,0),saturate(lerp(0.0-rS1, rS1, rVec))); //red
rainCol.rgb = lerp(rainCol,half3(0,1.0,0),saturate(lerp(0.0-rS1-0.2, rS1,rVec))); //green
rainCol.rgb = lerp(rainCol,half3(0,0,1.0),saturate(lerp(0.0-rS1-1.0, rS1,rVec))); //blue
rainCol.rgb = lerp(rainCol,half3(0.2,0.15,0.1),saturate(lerp(0.0-rS1-1.0, rS1,rVec)));

fCol.rgb = lerp(fCol.rgb, fCol.rgb - (rainCol.r*0.5) + rainCol*2, saturate(dot(Tenkoku_Vec_SunFwd.xyz, half3(0,1,0)-0.1)) * saturate(lerp(1.0,-2.0,fogFac)) * _tenkoku_rainbowFac1 * rFac );

//secondary
half rS2 = 8;
rainCol2.rgb = lerp(rainCol2,half3(0.0,0,1.0),saturate(lerp(0.0-rS2, rS2, rVec2))); //blue
rainCol2.rgb = lerp(rainCol2,half3(0,1.0,0),saturate(lerp(0.0-rS2-0.2, rS2,rVec2))); //green
rainCol2.rgb = lerp(rainCol2,half3(1.2,0,0.0),saturate(lerp(0.0-rS2-1.0, rS2,rVec2))); //red
rainCol2.rgb = lerp(rainCol2,0,saturate(lerp(0.0-rS2-1.0, rS2,rVec2)));

fCol.rgb = lerp(fCol.rgb, fCol.rgb - (rainCol2.r*0.5) + rainCol2*2, saturate(dot(Tenkoku_Vec_SunFwd.xyz, half3(0,1,0))) * saturate(lerp(0.8,-5.0,fogFac)) * (_tenkoku_rainbowFac2 * rFac * 0.73) );



//bloom moon
//if (dpth >= 1.0){
//	half mDot = saturate(dot(Tenkoku_Vec_MoonFwd.xyz, normalize( i.interpolatedRay.xyz)));
//	half mVec1 = saturate(lerp(0.0,4.0,mDot - 0.991));
//	half mVec2 = saturate(lerp(0.0,4.0,mDot - 0.995));
//	fCol.rgb = fCol.rgb + (fCol.rgb * mVec1 * 50) + (fCol.rgb * mVec2 * 50);
//}


//float blmFac = 0.0;
//if (dpth >= 1.0){
//	blmFac = lerp(1, 0, saturate(lerp(0.0,1.0, ((dot(half3(0,1,0), i.interpolatedRay.xyz)*0.0002)))));
//}
//fCol.rgb = lerp(fCol.rgb, fCol.rgb + lerp(0, pow(fCol.rgb,0.9), 1), saturate(blmFac) );



//fCol = skyColor;

//temp sun brighten
//float sDir = saturate(dot(Tenkoku_Vec_SunFwd.xyz, normalize( i.interpolatedRay.xyz)));
//float sFac = lerp(0.0,0.1,saturate(sDir-0.9));
//sFac += lerp(0.0,0.2,saturate(sDir-0.92));
//sFac += lerp(0.0,0.1,sDir-0.94);
//sFac += lerp(0.0,0.1,sDir-0.96);
//sFac += lerp(0.0,0.1,sDir-0.97);
//sFac += lerp(0.0,0.1,sDir-0.98);
//sFac += lerp(0.0,0.1,sDir-0.99);
//sFac += lerp(0.0,100,saturate(sDir-0.999));
//sFac = sFac * _Tenkoku_AmbientGI;
//fCol = fCol * (1.0 + (max(max(fCol.r,fCol.g),fCol.b) * sFac));
//if (dpth >= 1.0){
//fCol = dpth;//max(0.0,lerp(-1.0,(1-fogFac) * 2.0,max(max(fCol.r,fCol.g),fCol.b)) * sFac);
//}

//EFFECTS OVERLAY
//half4 skyEffects = tex2D(_Tenkoku_ParticleTex, i.uv);
//half3 skyMask = saturate(skyEffects.rgb - half3(0,1,0));
//if (dpth == 1.0){
//	fCol.rgb = fCol.rgb + skyEffects.rgb;//lerp(_SetColor.rgb,skyEffects.rgb,max(max(skyMask.r,skyMask.g),skyMask.b));// * (1.0-fogFac);//lerp(fCol.rgb,skyEffects.rgb,skyEffects.a);
//}





		return fCol;
	}

ENDCG

SubShader
{

	Tags { "RenderType"="Opaque" }
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	// 0: distance + height
	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag (v2f i) : SV_Target { return ComputeFog (i, true, true); }
		ENDCG
	}


}

Fallback off

}

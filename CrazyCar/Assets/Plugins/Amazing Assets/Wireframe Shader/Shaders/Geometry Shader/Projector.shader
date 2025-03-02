Shader "Amazing Assets/Wireframe Shader/Dynamic (SM5)/Geometry Shader (Projector)"
{
	Properties 
	{
[WireframeCurvedWorldTitle] _CurvedWorldTitle("", Float) = 0
//[CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)
		 

		[HideInInspector] _Color("Color (RGB)", color) = (1, 1, 1, 1)
		[HideInInspector] _MainTex("Base (RGB)", 2D) = "white"{}		

		//Visual Options
		[WireframeTitle] _Wireframe_Title_V_Options("Default Visual Options", float) = 0  

		[NoScaleOffset] _ShadowTex ("Color (RGB) Mask (A)", 2D) = "white" {}
		[NoScaleOffset] _FalloffTex ("FallOff (A)", 2D) = "" {}


		//Wire S Options  
		[WireframeTitle]  _Wireframe_Title_S_Options("Wireframe Shader Options", float) = 0
		_WireframeShader_Thickness("Thickness", Range(0, 1)) = 0.01
		_WireframeShader_Smoothness("Smoothness", Range(0, 1)) = 0	
		_WireframeShader_Diameter("Diameter", Range(0, 1)) = 1
		[Toggle(WIREFRAME_NORMALIZE_EDGES_ON)] _Wireframe_NormalizeEdges("Normalize Edges", Float) = 0
		[Toggle(WIREFRAME_TRY_QUAD_ON)] _Wireframe_TryQuad("Try Quad", Float) = 0
			
		//Wire Options
		[WireframeHeader] _Wireframe_Title_W_Options("Base Options", float) = 0  
		_Wireframe_Color("Color (RGB) Trans (A)", color) = (1, 0, 0, 1)
		[WireframePositiveFloat]_Wireframe_ColorEmissionStrength("Emission Strength", float) = 0

		//Transparency          
		[WireframeHeader]		  _Wireframe_Transparency_M_Options("Transparency Options", float) = 0
		[WireframeDistanceFade]  _Wireframe_DistanceFade ("Distance Fade", Float) = 0
		[HideInInspector] _Wireframe_DistanceFadeStart("", Float) = 5
		[HideInInspector] _Wireframe_DistanceFadeEnd("", Float) = 10
	}
	
	Subshader 
	{
		Tags { "Queue"="Transparent" } 
		 
		Pass 
		{
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha 
			Offset -1, -1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			#include "../cginc/WireframeGeometryShaderPlatform.cginc"


			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
						
			
			#pragma shader_feature_local WIREFRAME_DISTANCE_FADE_ON

			#include "../cginc/WireframeBuiltinRPCore.cginc"


			struct vInput
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;						
			};

			struct v2f_surf
			{
				float4 pos : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;

				fixed3 custompack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;

				float3 objectPos : TEXCOORD4;

				UNITY_FOG_COORDS(5)				
			};


			#pragma shader_feature_local WIREFRAME_NORMALIZE_EDGES_ON
			#pragma shader_feature_local WIREFRAME_TRY_QUAD_ON

			#include "../cginc/WireframeGeometryShader.cginc"

			
			v2f_surf vert (vInput v)
			{
				v2f_surf o;


#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
     CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
#endif

				o.objectPos = v.vertex.xyz;
				
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uvShadow = mul (unity_Projector, v.vertex);
				o.uvFalloff = mul (unity_ProjectorClip, v.vertex);
				o.custompack2 = 0;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				UNITY_TRANSFER_FOG(o,o.pos);
				
				return o;
			}
			
			
			
			fixed4 frag (v2f_surf i) : SV_Target
			{				

				fixed4 projT = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				projT.a *= tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff)).a;
								
				half value = 1 - WireframeShaderReadTrangleMassFromUV(i.custompack2.xyz, _WireframeShader_Thickness, _WireframeShader_Smoothness, _WireframeShader_Diameter);
				half4 res = projT * _Wireframe_Color;
				res.rgb *= _Wireframe_ColorEmissionStrength;
				res.a = lerp(projT.a * _Wireframe_Color.a, 0, value);


				//Distance Fade
				#ifdef WIREFRAME_DISTANCE_FADE_ON
					res.a *= WireframeShaderDistanceFade(_WorldSpaceCameraPos, i.worldPos, _Wireframe_DistanceFadeStart, _Wireframe_DistanceFadeEnd);;
				#endif

				
			
				UNITY_APPLY_FOG(i.fogCoord, res);			


				return res;
			}
			ENDCG
		} 
	}
}

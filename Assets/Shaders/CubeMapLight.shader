
Shader "Salesforce/CubemapLight"
{
	Properties
	{
		_TextureSample3("Cubemap", CUBE) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		_Float1("Intensity", Range( 0 , 1)) = 0.247
		_TextureSample1("Texture", 2D) = "white" {}
		_Color1("Color", Color) = (1,1,1,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf BlinnPhong addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_TextureSample1;
			float3 worldRefl;
			INTERNAL_DATA
		};

		uniform sampler2D _TextureSample1;
		uniform float4 _Color1;
		uniform float _Float1;
		uniform samplerCUBE _TextureSample3;

		void surf( Input input , inout SurfaceOutput output )
		{
			float4 FLOATToCOLOR260=_Float1;
			float4 FLOATToFLOAT4200=( 1.0 - _Float1 );
			output.Emission = ( ( ( saturate( ( tex2D( _TextureSample1,input.uv_TextureSample1) * _Color1 ) )) * FLOATToCOLOR260 ) + ( FLOATToFLOAT4200 * texCUBE( _TextureSample3,WorldReflectionVector( input , float3( 0,0,0 ) )) ) ).xyz;
		}

		ENDCG
	}
	Fallback "Diffuse"
}

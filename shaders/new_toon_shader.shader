
HEADER
{
	Description = "";
}

FEATURES
{
    #include "common/features.hlsl"
}

COMMON
{
#ifndef S_ALPHA_TEST
#define S_ALPHA_TEST 0
#endif
#ifndef S_TRANSLUCENT
#define S_TRANSLUCENT 0
#endif

	#include "common/shared.hlsl"

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "sbox_pixel.fxc"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/pixel.color.blending.hlsl"
	#include "common/proceedural.hlsl"

	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Color, Srgb, 8, "None", "_color", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( ColorTint, Srgb, 8, "None", "_color", ",0/0", Default4( 1.00, 0.00, 0.00, 1.00 ) );
	CreateInputTexture2D( Mask, Srgb, 8, "None", "_color", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Crosshatch, Srgb, 8, "None", "_color", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( Normal, Srgb, 8, "None", "_normal", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateInputTexture2D( AO, Srgb, 8, "None", "_ao", ",0/0", Default4( 0.00, 0.00, 0.00, 0.00 ) );
	CreateTexture2DWithoutSampler( g_tColor ) < Channel( RGBA, Box( Color ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tColorTint ) < Channel( RGBA, Box( ColorTint ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tMask ) < Channel( RGBA, Box( Mask ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tCrosshatch ) < Channel( RGBA, Box( Crosshatch ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( Normal ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	CreateTexture2DWithoutSampler( g_tAO ) < Channel( RGBA, Box( AO ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m;
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = TransformNormal( i, float3( 0, 0, 1 ) );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;

		float local0 = dot( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs ), i.vNormalWs.xyz );
		float local1 = local0 * -2;
		float local2 = step( -0.65, local1 );
		float4 local3 = float4( 0, 0, 0, 1 );
		float4 local4 = float4( local2, local2, local2, local2 ) + local3;
		float2 local5 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local6 = Tex2DS( g_tColor, g_sSampler0, local5 );
		float4 local7 = Tex2DS( g_tColorTint, g_sSampler0, i.vTextureCoords.xy );
		float2 local8 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local9 = Tex2DS( g_tMask, g_sSampler0, local8 );
		float4 local10 = saturate( lerp( local6, local6*local7, local9 ) );
		float4 local11 = i.vPositionSs / float4( 350, 350, 350, 350 );
		float4 local12 = Tex2DS( g_tCrosshatch, g_sSampler0, local11.xy );
		float local13 = local12.r;
		float4 local14 = floor( local4 );
		float local15 = dot( i.vNormalWs.xyz, CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs ) );
		float local16 = local15 * 2.2;
		float local17 = step( 1.55, local16 );
		float4 local18 = float4( 0.3526, 0.3526, 0.3526, 1 );
		float4 local19 = float4( local17, local17, local17, local17 ) + local18;
		float4 local20 = saturate( lerp( local14, local19, local2 ) );
		float4 local21 = saturate( lerp( float4( local13, local13, local13, local13 ), local20, local1 ) );
		float4 local22 = local21 + local19;
		float local23 = dot( i.vNormalWs.xyz, CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs ) );
		float local24 = local23 * 1;
		float local25 = step( 0.5, local24 );
		float4 local26 = float4( local25, local25, local25, local25 ) + local7;
		float4 local27 = local22 * local26;
		float4 local28 = local27 * float4( 1, 1, 1, 1 );
		float4 local29 = saturate( lerp( local10, Overlay_blend( local10, local28 ), 1 ) );
		float4 local30 = local4 + local29;
		float4 local31 = saturate( local30 );
		float4 local32 = local29 / float4( 20, 20, 20, 20 );
		float4 local33 = local4 / float4( 4, 4, 4, 4 );
		float4 local34 = local32 + local33;
		float2 local35 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local36 = Tex2DS( g_tNormal, g_sSampler0, local35 );
		float2 local37 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local38 = Tex2DS( g_tAO, g_sSampler0, local37 );

		m.Albedo = local31.xyz;
		m.Emission = local34.xyz;
		m.Normal = local36.xyz;
		m.AmbientOcclusion = local38.x;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}

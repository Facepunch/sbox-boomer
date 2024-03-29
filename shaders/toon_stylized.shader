//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Stylized Toon Shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"

    Feature( F_ILLUMINATE, 0..1, "Stylized Toon" );
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    VrForward();													// Indicates this shader will be used for main rendering
    Depth( "vr_depth_only.vfx" ); 									// Shader that will be used for shadowing and depth prepass
    ToolsVis( S_MODE_TOOLS_VIS ); 									// Ability to see in the editor
    ToolsWireframe( "vr_tools_wireframe.vfx" ); 					// Allows for mat_wireframe to work
    ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" ); 	// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================
PS
{
    StaticCombo( S_ILLUMINATE, F_ILLUMINATE, Sys( PC ) );
    float3 g_vColorTint < Attribute( "ColorTint" ); Default3( 1.0, 1.0, 1.0 ); >;
    float4 g_vShadowTint< Default4(0.0f, 0.0f, 0.0f, 1.0f); UiType( Color ); UiGroup( "Toon,10/Shadow,10/1" ); >;
    float3 g_vDiffuseColor< Default3(1.0f, 1.0f, 1.0f); UiType( Color ); UiGroup( "Toon,10/Diffuse,10/1" );  >;
    float3 g_vSpecularColor< Default3(1.0f, 1.0f, 1.0f); UiType( Color ); UiGroup( "Toon,10/Specular,10/1" ); >;
    float3 g_vRimColor< Default3(1.0f, 1.0f, 1.0f); UiType( Color ); UiGroup( "Toon,10/Rim Light,10/1" ); >;
    float g_flSpecularIntensive< Default(1.0f); Range(0.0f, 1.0f); UiGroup( "Toon,10/Specular,10/1" ); >;
    float g_flShininess< Default(0.5f); Range(0.0f, 1.0f); UiGroup( "Toon,10/Specular,10/1" ); >;
    float g_flRimPower< Default(3.0f); Range(0.0f, 10.0f); UiGroup( "Toon,10/Rim Light,10/2" );>;
    float g_flAttenuation< Default(1.0f); Range(0.0f, 1.0f); UiGroup( "Toon,10/Attenuation,10/1" );  >;
    #if S_ILLUMINATE
        float g_flIlluminateAmount< Default(0.0f); Range(0.0f, 1.0f); UiGroup( "Toon,10/Diffuse,10/1" );  >;
    #endif

    #include "common/pixel.hlsl"
	
    class ShadingModelStylizedToon : ShadingModel
    {
        float3 Albedo;
        float3 NormalWs;
        float AO;
        float3 PositionWithOffsetWs;
        float3 PositionWs;
        float3 ViewRayWs;

        void Init( const PixelInput pixelInput, const Material material )
        {
            Albedo = material.Albedo;
            NormalWs = material.Normal;
            AO = material.AmbientOcclusion;

            PositionWithOffsetWs = pixelInput.vPositionWithOffsetWs.xyz;
            PositionWs = PositionWithOffsetWs + g_vCameraPositionWs;

            // View ray in World Space
            ViewRayWs = CalculatePositionToCameraDirWs( PositionWs );
        }
        
        //
        // Executed for every direct light
        //
        LightShade Direct( const LightData light )
 {
            LightShade shade;
            float fLight = pow( saturate(light.NdotL) * light.Visibility , 0.25f ) * light.Attenuation;

            float3 vHalfAngleDirWs = normalize(ViewRayWs + light.LightDir);

            float flNDotL = saturate( light.NdotL );
            float flNdotH = dot( vHalfAngleDirWs.xyz, NormalWs );
            float fSpecularLobe = saturate(flNdotH) * fLight;

            shade.Diffuse = fLight * Albedo * g_vColorTint * light.Color;
            shade.Specular = saturate( pow(fSpecularLobe, 100.0f) * 1000.0f ) * Albedo * g_vColorTint;
            return shade;
        }
        
        //
        // Executed for indirect lighting, combine ambient occlusion, etc.
        //
        LightShade Indirect()
        {
            LightShade shade;

            // Get a flat average ambient
            float3 vAmbientCube[6];
		    SampleLightProbeVolume( vAmbientCube, float3(0,0,0) );

            const float flAttenuation = 1.0f;

            float3 vClearColor = (Albedo * g_vColorTint) * vAmbientCube[0];
            float3 vDiffuseReflection = g_vDiffuseColor.rgb;

            float flRim = 1.0f - saturate( dot(ViewRayWs, NormalWs) );
            float3 vRimLighting = g_flAttenuation * vAmbientCube[0] * g_vRimColor * pow( flRim, g_flRimPower + 10.0f ) * 1000.0f;
            vClearColor *= AO;
            float3 vDiffuse = (vClearColor + (vRimLighting) * 1.0f) * vDiffuseReflection;

            #if S_ILLUMINATE
                vDiffuse += (Albedo * g_vColorTint) * g_flIlluminateAmount;
            #endif

            shade.Diffuse =  vDiffuse;
            float fSpecularLobe = ( 1.2f - dot( ViewRayWs, NormalWs ) ) * saturate(NormalWs.z) * 1.25f;
            shade.Specular = saturate( pow(fSpecularLobe, 1000.0f) * 10.0f );
            return shade;
        }

        float4 PostProcess( float4 vColor )
        {
            // Do nothing
            return vColor;
        }
    };

	//
	// Main
	//
	PixelOutput MainPs( PixelInput i )
	{
		Material m = GatherMaterial( i );
		ShadingModelStylizedToon sm;
		
        PixelOutput o;
        o.vColor.rgba = FinalizePixelMaterial( i, m, sm );
        return o;
	}
}

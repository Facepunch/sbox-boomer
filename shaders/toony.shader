//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "Toony Shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
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
    nointerpolation uint nIsShade : TEXCOORD15;
    float3 vOffset : TEXCOORD14;
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"
	//
	// Main
	//
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
        o.nIsShade = 0;
        o.vOffset = float3(0,0,1000);
		return FinalizeVertex( o );
	}
}

//
// Used for toon outline
//
GS
{
    //
    // Main
    //
    [maxvertexcount(6)]
    void MainGs(triangle in PixelInput vertices[3], inout TriangleStream<PixelInput> triStream)
    {
        const float flOutlineSize = 0.0f;
        
        PixelInput v[3];
        for ( int i = 0; i < 3; i++ )
        {
            triStream.Append(vertices[i]);
            v[i] = vertices[i];
        }
        
        // emit the flipped vertices
        triStream.RestartStrip();
        
        // color all vertices black
        // Convert these to world space
        for ( int i = 0; i < 3; i++ )
        {
            v[i].vPositionWs = v[i].vPositionWs + ( v[i].vNormalWs * flOutlineSize );
            v[i].vPositionWs += g_vHighPrecisionLightingOffsetWs.xyz;
            v[i].vPositionPs = Position3WsToPsMultiview( 0, v[i].vPositionWs ) + float4(0,0,1,0);
            v[i].nIsShade = 1;
        }

        triStream.Append(v[2]);
        triStream.Append(v[0]);
        triStream.Append(v[1]);
    }
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"
    
    float3 g_vColorTint < Attribute( "ColorTint" ); Default3( 1.0, 1.0, 1.0 ); >;
    
    class ShadingModelToon : ShadingModel
    {
        float3 Albedo;
        float3 NormalWs;
        float3 PositionWithOffsetWs;
        float3 PositionWs;
        float3 ViewRayWs;

        //
        // Consumes a material and converts it to the internal shading parameters,
        // That is more easily consumed by the shader.
        //
        // Inherited classes can expand this to whichever shading model they want.
        //
        void Init( const PixelInput pixelInput, const Material material )
        {
            Albedo = material.Albedo;
            NormalWs = material.Normal;

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

            //
            // Toon Ambient color
            //
            shade.Diffuse =  vAmbientCube[0] * Albedo * g_vColorTint;

            //
            // Toon rimlight
            //
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
        
        // Refetch albedo in lower mip to give better toon look
        // m.Albedo = Tex2DLevelS( g_tColor, TextureFiltering, i.vTextureCoords.xy, 10.0f );

        if( i.nIsShade == 1 )
        {
            PixelOutput o;
            o.vColor = float4(0,0,0,1);
            return o;
        }
        
		//
		// Declare which shading model we are going to use to calculate lighting
		//
		ShadingModelToon sm;
		
        PixelOutput o;
		o.vColor = FinalizePixelMaterial( i, m, sm );
        return o;
	}
}
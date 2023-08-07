Shader "CustomRP/Lit"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (0.5, 0.5, 0.5, 1.0)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "CustomLit"
            }

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "Assets/CustomRP/Shaders/LitPass.hlsl"
            
            ENDHLSL
        }
    }
}

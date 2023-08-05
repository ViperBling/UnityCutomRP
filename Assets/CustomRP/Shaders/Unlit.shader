Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "Assets/CustomRP/Shaders/UnlitPass.hlsl"
            
            ENDHLSL
        }
    }
}
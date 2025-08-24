// Plik: DepthMask.shader
Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ColorMask 0 // Nie rysuj kolorów, tylko głębię
        }
    }
}
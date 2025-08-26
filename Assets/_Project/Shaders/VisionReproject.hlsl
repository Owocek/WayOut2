// Assets/Shaders/Vision/VisionReproject.hlsl
// Wersja float
void VisionReproject_float(
    float3 worldPos,
    float4 VP0, float4 VP1, float4 VP2, float4 VP3,
    out float2 uv, out float depth01, out float insideFrustum)
{
    float4x4 VPmat;
    VPmat[0] = VP0;
    VPmat[1] = VP1;
    VPmat[2] = VP2;
    VPmat[3] = VP3;

    float4 clip = mul(VPmat, float4(worldPos, 1.0));
    float invW = 1.0 / max(1e-6, clip.w);

    uv = clip.xy * invW * 0.5 + 0.5;
    depth01 = clip.z * invW * 0.5 + 0.5;

    float inX = step(0.0, uv.x) * step(uv.x, 1.0);
    float inY = step(0.0, uv.y) * step(uv.y, 1.0);
    float inW = step(0.0, clip.w);
    insideFrustum = inX * inY * inW;
}

// Wersja half (niektóre preview kompilują w half)
void VisionReproject_half(
    half3 worldPos,
    half4 VP0, half4 VP1, half4 VP2, half4 VP3,
    out half2 uv, out half depth01, out half insideFrustum)
{
    float2 uvF; float depthF; float insideF;
    VisionReproject_float((float3)worldPos, (float4)VP0, (float4)VP1, (float4)VP2, (float4)VP3, uvF, depthF, insideF);
    uv = (half2)uvF;
    depth01 = (half)depthF;
    insideFrustum = (half)insideF;
}
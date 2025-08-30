using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class FogOfWarRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader fogCompositeShader; // Hidden/WayOut2/FogComposite
    [Range(0, 1)] public float darkness = 0.85f;
    [Range(0, 1)] public float desaturate = 0.6f;

    private Material _mat;
    private FogOfWarPass _pass;

    public override void Create()
    {
        if (fogCompositeShader != null)
            _mat = CoreUtils.CreateEngineMaterial(fogCompositeShader);

        _pass = new FogOfWarPass(_mat)
        {
            renderPassEvent = RenderPassEvent.AfterRendering
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_mat == null) return;
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        _mat.SetFloat("_Darkness", darkness);
        _mat.SetFloat("_Desaturate", desaturate);

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_mat);
        FogOfWarPass.ReleaseImportedHandle();
    }

    private class FogOfWarPass : ScriptableRenderPass
    {
        private class PassData { public Material mat; }

        private readonly Material _mat;
        private static RTHandle s_MaskRH; // wrapper na zewnętrzny RenderTexture

        public FogOfWarPass(Material mat) { _mat = mat; }

        public static void ReleaseImportedHandle()
        {
            if (s_MaskRH != null) { s_MaskRH.Release(); s_MaskRH = null; }
        }

        public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
        {
            if (_mat == null) return;

            var resources = frameData.Get<UniversalResourceData>();
            var camData   = frameData.Get<UniversalCameraData>();

            TextureHandle src = resources.activeColorTexture;

            // opis tymczasowego RT zgodny z kamerą
            var desc = camData.cameraTargetDescriptor;
            desc.msaaSamples     = 1;
            desc.depthBufferBits = 0;

            TextureHandle tmp = UniversalRenderer.CreateRenderGraphTexture(rg, desc, "FogComposite_Temp", false);

            // 1) Import zewnętrznego RenderTexture z MaskCamera
            var maskRT = VisionMaskCamera.CurrentRT;
            if (maskRT == null) return; // nie ma maski – pomiń pass

            // Stwórz/odśwież RTHandle owijający RT (utrzymujemy go statycznie i zwalniamy w Dispose)
            if (s_MaskRH == null || s_MaskRH.rt != maskRT)
            {
                ReleaseImportedHandle();
                s_MaskRH = RTHandles.Alloc(maskRT, name: "_VisionMask_RTHandle");
            }

            // Import do RenderGraph -> mamy TextureHandle
            TextureHandle maskTH = rg.ImportTexture(s_MaskRH);

            // PASS 1: composite src + mask -> tmp
            using (var builder = rg.AddRasterRenderPass<PassData>("FogComposite", out var passData))
            {
                passData.mat = _mat;

                // deklarujemy zależności RG (oba jako TextureHandle)
                builder.UseTexture(src,    AccessFlags.Read);
                builder.UseTexture(maskTH, AccessFlags.Read);
                builder.SetRenderAttachment(tmp, 0, AccessFlags.Write);

                builder.SetRenderFunc<PassData>((data, ctx) =>
                {
                    // dodatkowo zbindowanie globalne do nazwy użytej w shaderze
                    var rt = VisionMaskCamera.CurrentRT;
                    if (rt) ctx.cmd.SetGlobalTexture("_VisionMask", rt);

                    CoreUtils.DrawFullScreen(ctx.cmd, data.mat, shaderPassId: 0);
                });
            }

            // PASS 2: tmp -> src (kopiujemy z powrotem do koloru kamery)
            using (var builder = rg.AddRasterRenderPass<PassData>("FogComposite CopyBack", out var passData2))
            {
                passData2.mat = _mat;

                builder.UseTexture(tmp, AccessFlags.Read);
                builder.SetRenderAttachment(src, 0, AccessFlags.Write);

                builder.SetRenderFunc<PassData>((data, ctx) =>
                {
                    CoreUtils.DrawFullScreen(ctx.cmd, data.mat, shaderPassId: 1);
                });
            }
        }
    }
}
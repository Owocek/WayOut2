using UnityEngine;

public class VisionMaskCamera : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;   // your gameplay/top-down camera
    public Camera maskCamera;   // the helper camera that renders VisionGeometry

    [Header("Output")]
    public RenderTextureFormat format = RenderTextureFormat.R8; // single-channel mask is enough

    private static RenderTexture _rt;
    public static RenderTexture CurrentRT => _rt;

    private void OnEnable()
    {
        EnsureRT();
        SyncFromMain();
    }

    private void OnDisable()
    {
        if (_rt) _rt.Release();
        _rt = null;
    }

    private void LateUpdate()
    {
        EnsureRT();
        SyncFromMain();
        Shader.SetGlobalTexture("_VisionMask", _rt);
    }

    private void EnsureRT()
    {
        int w = Screen.width;
        int h = Screen.height;
        if (_rt == null || _rt.width != w || _rt.height != h || _rt.format != format)
        {
            if (_rt) _rt.Release();
            _rt = new RenderTexture(w, h, 0, format)
            {
                name = "VisionMask_RT",
                filterMode = FilterMode.Bilinear
            };
            _rt.Create();
            if (maskCamera) maskCamera.targetTexture = _rt;
        }
    }

    private void SyncFromMain()
    {
        if (!mainCamera || !maskCamera) return;

        maskCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        maskCamera.orthographic = mainCamera.orthographic;
        if (mainCamera.orthographic)
            maskCamera.orthographicSize = mainCamera.orthographicSize;
        else
            maskCamera.fieldOfView = mainCamera.fieldOfView;

        // In Inspector:
        // - Clear Flags = Solid Color (black)
        // - Culling Mask = VisionGeometry
        // - Background = black (alpha 1)
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class Vision : MonoBehaviour
{
    [Header("Refs")]
    public Camera visionCamera;                       // VisionCamera
    public DecalProjector visionDecal;                // VisionDecal
    public RenderTexture visionRT;                    // Vision_RT

    [Header("Params")]
    public float viewRadius = 20f;
    public float epsilon = 0.005f;

    // Nazwy MUSZĄ zgadzać się z "Reference" w Shader Graph
    static readonly int DepthTexID = Shader.PropertyToID("_DepthTexture");
    static readonly int VP0_ID     = Shader.PropertyToID("_VisionVP0");
    static readonly int VP1_ID     = Shader.PropertyToID("_VisionVP1");
    static readonly int VP2_ID     = Shader.PropertyToID("_VisionVP2");
    static readonly int VP3_ID     = Shader.PropertyToID("_VisionVP3");
    static readonly int CamPosID   = Shader.PropertyToID("_VisionCamPos");
    static readonly int RadiusID   = Shader.PropertyToID("_Radius");
    static readonly int EpsID      = Shader.PropertyToID("_Epsilon");

    void Reset()
    {
        if (!visionCamera) visionCamera = GetComponentInChildren<Camera>(true);
        if (!visionDecal)  visionDecal  = GetComponentInChildren<DecalProjector>(true);
    }

    void OnEnable()  { Hookup(); }
    void OnValidate(){ Hookup(); }

    void Hookup()
    {
        if (visionCamera && visionRT && visionCamera.targetTexture != visionRT)
            visionCamera.targetTexture = visionRT;
    }

    void Update()
    {
        if (!visionCamera || !visionDecal) return;

        // zawsze pracujemy na aktualnym materiale z Decal Projectora
        var mat = visionDecal.material;
        if (!mat) return;

        // RT do materiału
        if (visionRT) mat.SetTexture(DepthTexID, visionRT);

        // VP = GPUProjection * View
        var proj = GL.GetGPUProjectionMatrix(visionCamera.projectionMatrix, true);
        Matrix4x4 vp = proj * visionCamera.worldToCameraMatrix;

        // parametry do materiału
        mat.SetVector(VP0_ID, vp.GetRow(0));
        mat.SetVector(VP1_ID, vp.GetRow(1));
        mat.SetVector(VP2_ID, vp.GetRow(2));
        mat.SetVector(VP3_ID, vp.GetRow(3));
        mat.SetVector(CamPosID, visionCamera.transform.position);
        mat.SetFloat(RadiusID, viewRadius);
        mat.SetFloat(EpsID, epsilon);

        // rozmiar i pozycja decal'a (top-down)
        visionDecal.size = new Vector3(viewRadius * 2f, viewRadius * 2f, 100f);
        var p = transform.position;
        visionDecal.transform.SetPositionAndRotation(
            new Vector3(p.x, 10f, p.z),
            Quaternion.Euler(90f, 0f, 0f)
        );
    }

#if UNITY_EDITOR
    [ContextMenu("Log Vision Material State")]
    void LogMat()
    {
        var mat = visionDecal ? visionDecal.material : null;
        Debug.Log($"Vision mat={(mat?mat.name:"null")} RT={(visionRT?visionRT.name:"null")} cam={(visionCamera?visionCamera.name:"null")}");
    }
#endif
}
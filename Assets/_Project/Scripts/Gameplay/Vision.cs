using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class Vision : MonoBehaviour
{
    [Header("Refs")]
    public Camera visionCamera; // VisionCamera
    public UnityEngine.Rendering.Universal.DecalProjector visionDecal; // VisionDecal
    public Material visionMat;  // VisionCone_Material_SG
    public RenderTexture visionRT; // Vision_RT

    [Header("Params")]
    public float viewRadius = 20f;
    public float epsilon = 0.005f;

    // Uwaga: te nazwy MUSZĄ pokrywać się z "Reference" w Shader Graph
    static readonly int DepthTexID = Shader.PropertyToID("_DepthTexture");
    static readonly int VP0 = Shader.PropertyToID("_VisionVP0");
    static readonly int VP1 = Shader.PropertyToID("_VisionVP1");
    static readonly int VP2 = Shader.PropertyToID("_VisionVP2");
    static readonly int VP3 = Shader.PropertyToID("_VisionVP3");
    static readonly int CamPosID = Shader.PropertyToID("_VisionCamPos");
    static readonly int RadiusID = Shader.PropertyToID("_Radius");
    static readonly int EpsilonID = Shader.PropertyToID("_Epsilon");

    void Reset()
    {
        if (!visionCamera) visionCamera = GetComponentInChildren<Camera>();
        if (!visionDecal) visionDecal = GetComponentInChildren<UnityEngine.Rendering.Universal.DecalProjector>();
    }

    void OnEnable()
    {
        Hookup();
    }

    void OnValidate()
    {
        Hookup();
    }

    void Hookup()
    {
        if (!visionCamera) return;

        if (visionRT && visionCamera.targetTexture != visionRT)
            visionCamera.targetTexture = visionRT;

        if (!visionDecal)
            visionDecal = GetComponentInChildren<UnityEngine.Rendering.Universal.DecalProjector>();

        if (!visionMat && visionDecal)
            visionMat = visionDecal.material;

        if (visionMat && visionRT)
            visionMat.SetTexture(DepthTexID, visionRT);
    }

    void Update()
    {
        if (!visionCamera || !visionMat || !visionDecal) return;

        var proj = GL.GetGPUProjectionMatrix(visionCamera.projectionMatrix, true);
        Matrix4x4 vp = proj * visionCamera.worldToCameraMatrix;

        visionMat.SetVector(VP0, vp.GetRow(0));
        visionMat.SetVector(VP1, vp.GetRow(1));
        visionMat.SetVector(VP2, vp.GetRow(2));
        visionMat.SetVector(VP3, vp.GetRow(3));
        visionMat.SetVector(CamPosID, visionCamera.transform.position);
        visionMat.SetFloat(RadiusID, viewRadius);
        visionMat.SetFloat(EpsilonID, epsilon);

        // Skala i pozycja Decala
        visionDecal.size = new Vector3(viewRadius * 2f, viewRadius * 2f, 100f);
        var t = visionDecal.transform;
        var pos = transform.position;
        t.SetPositionAndRotation(new Vector3(pos.x, 10f, pos.z), Quaternion.Euler(90f, 0f, 0f));
    }
}
using UnityEngine;
using UnityEngine.Rendering.Universal;

using UnityEngine.Rendering.Universal.Internal;

public class Vision : MonoBehaviour
{
    [SerializeField] Camera visionCamera;
    [SerializeField] UnityEngine.Rendering.Universal.DecalProjector visionDecal;
    [SerializeField] Material visionMat; // przypnij VisionCone_Material_SG
    [SerializeField] RenderTexture visionRT; // Vision_RT

    [Header("Params")]
    public float viewRadius = 20f;
    [Range(1, 179)] public float viewAngle = 110f;

    static readonly int VisionVP_ID = Shader.PropertyToID("_VisionVP");
    static readonly int VisionEye_ID = Shader.PropertyToID("_VisionEye");
    static readonly int VisionFwd_ID = Shader.PropertyToID("_VisionFwd");
    static readonly int Radius_ID = Shader.PropertyToID("_Radius");
    static readonly int CosHalfAngle_ID = Shader.PropertyToID("_CosHalfAngle");
    static readonly int DepthTexture_ID = Shader.PropertyToID("_DepthTexture");

    void Reset()
    {
        if (!visionMat && visionDecal) visionMat = visionDecal.material;
    }

    void LateUpdate()
    {
        if (!visionCamera || !visionDecal || !visionMat) return;

        // 1) Podpinamy RT
        if (visionRT) visionMat.SetTexture(DepthTexture_ID, visionRT);

        // 2) VP VisionCamera
        var vp = visionCamera.projectionMatrix * visionCamera.worldToCameraMatrix;
        visionMat.SetMatrix(VisionVP_ID, vp);

        // 3) Pozycja i kierunek "oczu"
        var t = visionCamera.transform;
        visionMat.SetVector(VisionEye_ID, t.position);
        visionMat.SetVector(VisionFwd_ID, t.forward);

        // 4) Parametry stożka
        visionMat.SetFloat(Radius_ID, viewRadius);
        visionMat.SetFloat(CosHalfAngle_ID, Mathf.Cos(viewAngle * Mathf.Deg2Rad * 0.5f));

        // 5) Rozmiar i orientacja Decala (projektor z góry)
        visionDecal.size = new Vector3(viewRadius * 2f, 1f, viewRadius * 2f); // width, height, depth
                                                                              // Ustaw decal nad graczem; zakładam, że Y to wysokość świata
        var p = transform.position;
        visionDecal.transform.position = new Vector3(p.x, visionDecal.transform.position.y, p.z);
        // Obrót tak, by oś Z decal’a “celowała” w kierunku patrzenia (po X=90 deg)
        visionDecal.transform.rotation = Quaternion.Euler(90f, t.eulerAngles.y, 0f);
    }
}
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class VisionSetupDump
{
    [MenuItem("Tools/Vision/Export Setup Report")]
    public static void ExportReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Vision Setup Report ===");
        sb.AppendLine("Unity: " + Application.unityVersion);
        sb.AppendLine("Active Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // URP Asset
        var rpa = GraphicsSettings.currentRenderPipeline;
        sb.AppendLine("\n[RenderPipelineAsset]");
        sb.AppendLine("RPA: " + (rpa ? $"{rpa.name} ({rpa.GetType().FullName})" : "null"));
        if (rpa)
        {
            var rpaPath = AssetDatabase.GetAssetPath(rpa);
            sb.AppendLine("RPA Path: " + rpaPath);

            var urp = rpa as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                sb.AppendLine($"HDR={urp.supportsHDR}, MSAA={urp.msaaSampleCount}, RenderScale={urp.renderScale}");

                // Renderer list + features (prywatne pola – refleksja)
                var fiList = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
                var fiDefault = typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance);
                var list = fiList?.GetValue(urp) as ScriptableRendererData[];
                int defIndex = fiDefault != null ? (int)fiDefault.GetValue(urp) : 0;

                if (list != null)
                {
                    sb.AppendLine($"\n[RendererDataList] count={list.Length}, defaultIndex={defIndex}");
                    for (int i = 0; i < list.Length; i++)
                    {
                        var rd = list[i];
                        if (!rd) { sb.AppendLine($" - [{i}] null"); continue; }
                        sb.AppendLine($" - [{i}] {rd.name} ({rd.GetType().Name}) path={AssetDatabase.GetAssetPath(rd)}");

                        var fiFeatures = typeof(ScriptableRendererData).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
                        var features = fiFeatures?.GetValue(rd) as List<ScriptableRendererFeature>;
                        if (features != null && features.Count > 0)
                        {
                            foreach (var f in features)
                                sb.AppendLine("    feature: " + (f ? $"{f.name} ({f.GetType().Name})" : "null"));
                            bool hasDecal = features.Any(f => f && f.GetType().Name.ToLower().Contains("decal"));
                            sb.AppendLine("    has Decal feature: " + hasDecal);
                        }

                        var urd = rd as UniversalRendererData;
                        if (urd != null)
                        {
                            var fiMode = typeof(UniversalRendererData).GetField("m_RenderingMode", BindingFlags.NonPublic | BindingFlags.Instance);
                            var mode = fiMode != null ? fiMode.GetValue(urd) : null;
                            sb.AppendLine("    RenderingMode: " + (mode != null ? mode.ToString() : "unknown"));

                            var fiDecalLayers = typeof(UniversalRendererData).GetField("m_DecalLayers", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (fiDecalLayers != null) sb.AppendLine("    DecalLayers: " + fiDecalLayers.GetValue(urd));
                        }
                    }
                }
            }
        }

        // Kamery
        sb.AppendLine("\n[Cameras]");
        var cams = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cam in cams)
        {
            var cad = cam.GetUniversalAdditionalCameraData();
            string rendererType = cad != null && cad.scriptableRenderer != null ? cad.scriptableRenderer.GetType().Name : "null";
            sb.AppendLine($" - {cam.name}: type={(cam.orthographic ? "Ortho" : "Persp")}, size/fov={(cam.orthographic ? cam.orthographicSize : cam.fieldOfView)}, cullingMask={LayerMaskToString(cam.cullingMask)}, targetTexture={(cam.targetTexture ? cam.targetTexture.name : "null")}, rendererType={rendererType}");
        }

        // Decale
        sb.AppendLine("\n[DecalProjectors]");
        var decals = Object.FindObjectsByType<UnityEngine.Rendering.Universal.DecalProjector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var d in decals)
        {
            sb.AppendLine($" - {d.name}: size={d.size}, pos={d.transform.position}, rot={d.transform.rotation.eulerAngles}, layer={LayerMask.LayerToName(d.gameObject.layer)}");
            var mat = d.material;
            if (mat)
            {
                sb.AppendLine($"   mat={mat.name}, shader={mat.shader.name}");
                foreach (string p in new[] { "_DepthTexture", "_VisionVP0","_VisionVP1","_VisionVP2","_VisionVP3","_VisionCamPos","_Radius","_Epsilon" })
                {
                    int id = Shader.PropertyToID(p);
                    sb.AppendLine($"   has {p} = {mat.HasProperty(id)}");
                }
            }
            else sb.AppendLine("   mat=null");
        }

        // RenderTextures
        sb.AppendLine("\n[RenderTextures]");
        var guids = AssetDatabase.FindAssets("t:RenderTexture");
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if (!rt) continue;
            sb.AppendLine($" - {rt.name}: {rt.width}x{rt.height} fmt={rt.graphicsFormat} depth={rt.depthStencilFormat} AA={rt.antiAliasing} path={path}");
        }

        // MeshRenderers (Receive Decals)
        sb.AppendLine("\n[MeshRenderers sample: ReceiveDecals]");
        var mrs = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Take(60).ToArray();
        foreach (var mr in mrs)
        {
            var mat = mr.sharedMaterial;
            bool hasRecv = mat && mat.HasProperty("_ReceiveDecals");
            string recvVal = hasRecv ? mat.GetFloat("_ReceiveDecals").ToString() : "-";
            sb.AppendLine($" - {mr.name} (layer={LayerMask.LayerToName(mr.gameObject.layer)}): mat={(mat?mat.name:"null")}, shader={(mat?mat.shader.name:"null")}, ReceiveDecalsProp={hasRecv} val={recvVal}");
        }

        // Zapis
        var file = Path.Combine(Application.dataPath, "Vision_Setup_Report.txt");
        File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
        Debug.Log("Vision report saved to: " + file);
        EditorUtility.RevealInFinder(file);
    }

    static string LayerMaskToString(int mask)
    {
        if (mask == ~0) return "Everything";
        var names = new List<string>();
        for (int i = 0; i < 32; i++)
            if ((mask & (1 << i)) != 0)
                names.Add(LayerMask.LayerToName(i));
        return string.Join(",", names);
    }
}
#endif
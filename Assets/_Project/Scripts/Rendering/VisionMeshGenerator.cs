using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionMeshGenerator : MonoBehaviour
{
    [Header("Viewer (player/eye)")]
    public Transform eye;              // player head/eye transform
    public float eyeHeight = 1.7f;

    [Header("FOV / range / resolution")]
    [Range(0f, 360f)] public float fieldOfView = 360f;
    public float maxDistance = 30f;
    [Range(64, 2048)] public int rayCount = 720; // 0.5° steps for 360°

    [Header("Collision")]
    public LayerMask occluders; // set to "Walls" layer

    private Mesh _mesh;

    private void Awake()
    {
        _mesh = new Mesh { name = "VisionMesh" };
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    private void LateUpdate()
    {
        if (!eye) return;
        Generate();
    }

    private void Generate()
    {
        var center = eye.position;
        center.y = eyeHeight;

        int steps = Mathf.Max(3, Mathf.CeilToInt(rayCount * (fieldOfView / 360f)));
        float angleStep = fieldOfView / steps;
        float startAngle = eye.eulerAngles.y - fieldOfView * 0.5f;

        // vertices: [0] = center, then hit points
        List<Vector3> verts = new List<Vector3>(steps + 2) { transform.InverseTransformPoint(center) };
        List<int> tris = new List<int>(steps * 3);

        for (int i = 0; i <= steps; i++) // close the fan
        {
            float ang = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(ang), 0f, Mathf.Cos(ang)); // XZ plane
            Vector3 origin = center;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, occluders, QueryTriggerInteraction.Ignore))
            {
                var p = hit.point; p.y = center.y;
                verts.Add(transform.InverseTransformPoint(p));
            }
            else
            {
                var p = origin + dir * maxDistance; p.y = center.y;
                verts.Add(transform.InverseTransformPoint(p));
            }

            if (i > 0)
            {
                // triangles (0, i, i+1)
                tris.Add(0);
                tris.Add(i);
                tris.Add(i + 1);
            }
        }

        _mesh.Clear();
        _mesh.SetVertices(verts);
        _mesh.SetTriangles(tris, 0, true);
        _mesh.RecalculateBounds();
    }
}

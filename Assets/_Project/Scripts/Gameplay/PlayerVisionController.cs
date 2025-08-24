// Plik: PlayerVisionController.cs
using UnityEngine;
using UnityEngine.Rendering.Universal; // Potrzebne dla Decal Projector

public class PlayerVisionController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Camera fovCamera;
    [SerializeField] private DecalProjector visionDecal;

    [Header("Aiming")]
    [SerializeField] private Transform aimTarget; // Obiekt, który będzie się obracał

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Obracanie kamery FOV i Decala w kierunku myszki
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            Vector3 direction = worldPoint - transform.position;
            direction.y = 0;
            
            if (aimTarget != null)
            {
                aimTarget.forward = direction.normalized;
            }
        }
    }
}
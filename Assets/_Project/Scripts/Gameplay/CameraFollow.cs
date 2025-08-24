// Plik: Scripts/Gameplay/CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // Obiekt, za którym kamera ma podążać (nasz gracz)
    [SerializeField] private float smoothSpeed = 0.125f; // Szybkość, z jaką kamera dogania cel
    [SerializeField] private Vector3 offset; // Odsunięcie kamery od celu (np. żeby była wyżej i z tyłu)

    private void LateUpdate()
    {
        if (target == null)
        {
            return; // Nie rób nic, jeśli nie ma celu
        }

        // Oblicz docelową pozycję kamery
        Vector3 desiredPosition = target.position + offset;
        
        // Płynnie interpoluj pozycję kamery
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Zastosuj nową pozycję
        transform.position = smoothedPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
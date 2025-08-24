// Plik: Scripts/Core/GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Wzorzec Singleton zapewniający łatwy, globalny dostęp
    public static GameManager Instance { get; private set; }

    [Header("Controllers")]
    [SerializeField] private MazeController mazeController;

    [Header("Game Elements")]
    [SerializeField] private GameObject playerObject;

    [Header("Camera")]
    [SerializeField] private CameraFollow mainCameraFollow;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Na początku gry gracz jest nieaktywny, a UI jest widoczne (zarządzane przez UIManager)
        playerObject.SetActive(false);
    }

    // Metoda wywoływana przez UIManager po kliknięciu przycisku start
    public void StartGame(MazeSettings settings)
    {
        playerObject.SetActive(true);
    
        if (mainCameraFollow != null)
        {
            mainCameraFollow.SetTarget(playerObject.transform);
        }
    
    mazeController.BeginGeneration(settings.width, settings.height);
    }
}
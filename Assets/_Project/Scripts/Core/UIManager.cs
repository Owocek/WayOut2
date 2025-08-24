// Plik: Scripts/Core/UIManager.cs
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject startMenuPanel;
    
    void Start()
    {
        startMenuPanel.SetActive(true);
    }
    
    // Ta metoda będzie podpięta pod przyciski w menu startowym.
    // Każdy przycisk będzie miał przypisany w Inspektorze inny MazeSettings.
    public void OnStartGameButtonClicked(MazeSettings settings)
    {
        if (settings == null)
        {
            Debug.LogError("Ustawienia labiryntu nie są przypisane do przycisku!");
            return;
        }

        startMenuPanel.SetActive(false);

        // Informuje GameManager o rozpoczęciu gry, przekazując wybrane ustawienia.
        GameManager.Instance.StartGame(settings);
    }
}
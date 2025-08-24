// Plik: Scripts/Core/DayNightCycle.cs (Wersja Zintegrowana z FOV)
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Długość pełnego cyklu dnia i nocy w sekundach.")]
    [SerializeField] private float cycleDurationSeconds = 240f; // 4 minuty

    [Header("Field of View Control")]
    [Tooltip("Materiał stożka pola widzenia, którego kolor będziemy zmieniać.")]
    [SerializeField] private Material fovMaterial;
    [Tooltip("Kolor stożka FOV w zależności od pory dnia.")]
    [SerializeField] private Gradient fovColor;

    [Header("Ambient Light Control")]
    [Tooltip("Kolor światła otoczenia w zależności od pory dnia.")]
    [SerializeField] private Gradient ambientColor;

    private float currentTimeOfDay = 0.25f; // Zaczynamy rano

    // ZMIANA: Przechowujemy referencję do property shadera, aby było wydajniej
    private int colorPropertyID;

    private void Start()
    {
        // ZMIANA: Pobieramy ID property "_BaseColor" lub "_Color" shadera
        // To jest znacznie szybsze niż używanie stringa w Update()
        if (fovMaterial.HasProperty("_BaseColor"))
        {
            colorPropertyID = Shader.PropertyToID("_BaseColor");
        }
        else if (fovMaterial.HasProperty("_Color"))
        {
            colorPropertyID = Shader.PropertyToID("_Color");
        }
    }

    void Update()
    {
        // 1. Aktualizacja czasu (bez zmian)
        float timeStep = Time.deltaTime / cycleDurationSeconds;
        currentTimeOfDay += timeStep;
        if (currentTimeOfDay >= 1f)
        {
            currentTimeOfDay -= 1f;
        }

        // 2. Aktualizacja oświetlenia (NOWA LOGIKA)
        UpdateLighting(currentTimeOfDay);
    }

    private void UpdateLighting(float timePercent)
    {
        // ZMIANA: Ustawiamy kolor światła otoczenia
        RenderSettings.ambientLight = ambientColor.Evaluate(timePercent);
        
        // ZMIANA: Ustawiamy kolor materiału stożka FOV
        if (fovMaterial != null && colorPropertyID != 0)
        {
            fovMaterial.SetColor(colorPropertyID, fovColor.Evaluate(timePercent));
        }
    }
}
// Plik: Scripts/ScriptableObjects/MazeSettings.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewMazeSettings", menuName = "Maze/Maze Settings")]
public class MazeSettings : ScriptableObject
{
    [Tooltip("Szerokość labiryntu. Zostanie automatycznie zaokrąglona do najbliższej liczby nieparzystej.")]
    public int width = 21;

    [Tooltip("Wysokość labiryntu. Zostanie automatycznie zaokrąglona do najbliższej liczby nieparzystej.")]
    public int height = 11;
}
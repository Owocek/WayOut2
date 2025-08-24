// Plik: Scripts/Gameplay/MazeGenerator.cs
using System;
using UnityEngine; // Używamy silnika Unity dla Random

public class MazeGenerator
{
    public Maze Generate(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("Wymiary labiryntu muszą być dodatnie.");
        }

        // Zapewnia, że wymiary są nieparzyste, co jest kluczowe dla algorytmu
        width = width % 2 == 0 ? width + 1 : width;
        height = height % 2 == 0 ? height + 1 : height;
        
        var maze = new Maze(width, height);

        CarvePassages(1, 1, maze);
        return maze;
    }

    private void CarvePassages(int currentX, int currentY, Maze maze)
    {
        maze.Grid[currentX, currentY] = CellType.Path;

        Vector2Int[] directions = {
            new Vector2Int(0, 2),  // Góra
            new Vector2Int(2, 0),  // Prawo
            new Vector2Int(0, -2), // Dół
            new Vector2Int(-2, 0)  // Lewo
        };

        // Tasowanie Fisher-Yates - zero alokacji pamięci!
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, directions.Length);
            Vector2Int temp = directions[randomIndex];
            directions[randomIndex] = directions[i];
            directions[i] = temp;
        }

        foreach (var dir in directions)
        {
            int nextX = currentX + dir.x;
            int nextY = currentY + dir.y;

            if (nextX > 0 && nextX < maze.Width - 1 && nextY > 0 && nextY < maze.Height - 1)
            {
                if (maze.Grid[nextX, nextY] == CellType.Wall)
                {
                    maze.Grid[currentX + dir.x / 2, currentY + dir.y / 2] = CellType.Path;
                    CarvePassages(nextX, nextY, maze);
                }
            }
        }
    }
}
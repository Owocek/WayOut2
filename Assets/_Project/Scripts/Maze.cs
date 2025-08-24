// Plik: Scripts/Maze.cs
public enum CellType
{
    Wall,
    Path
}

public class Maze
{
    public int Width { get; }
    public int Height { get; }
    public CellType[,] Grid { get; }

    public Maze(int width, int height)
    {
        Width = width;
        Height = height;
        Grid = new CellType[width, height];
        // Tablica jest automatycznie inicjalizowana wartością domyślną dla enum,
        // czyli pierwszym elementem (Wall = 0).
    }
}
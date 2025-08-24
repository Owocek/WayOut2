// Plik: Scripts/Core/MazeController.cs
using UnityEngine;
using System.Collections.Generic;

public class MazeController : MonoBehaviour
{
    [Header("Prefabs & Visuals")]
    [SerializeField] private GameObject startPrefab;
    [SerializeField] private GameObject endPrefab;
    [SerializeField] private Material backgroundMaterial;

    [Header("Poolers")]
    [SerializeField] private ObjectPooler wallPooler;

    [Header("Game Elements")]
    [SerializeField] private GameObject playerObject;
    
    private readonly List<GameObject> activeMazeObjects = new List<GameObject>();
    private Transform mazeParent;
    
    public void BeginGeneration(int width, int height)
    {
        ClearMaze();
        
        var generator = new MazeGenerator();
        var maze = generator.Generate(width, height);
        
        DrawMaze(maze);
        PlaceStartAndEndPoints(maze);
    }
    
    private void DrawMaze(Maze maze)
    {
        if (mazeParent == null)
        {
            mazeParent = new GameObject("Generated Maze").transform;
        }

        for (int x = 0; x < maze.Width; x++)
        {
            for (int z = 0; z < maze.Height; z++) // Używamy Z dla głębokości
            {
                if (maze.Grid[x, z] == CellType.Wall)
                {
                    GameObject wall = wallPooler.Get();
                    wall.transform.position = new Vector3(x, 0, z);
                    wall.transform.rotation = Quaternion.identity;
                    wall.transform.SetParent(mazeParent);
                    activeMazeObjects.Add(wall);
                }
            }
        }
        CreateBackground(maze.Width, maze.Height);
    }

    private void ClearMaze()
    {
        foreach (GameObject obj in activeMazeObjects)
        {
            if(obj.CompareTag("Wall"))
            {
                wallPooler.Return(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        activeMazeObjects.Clear();
    }
    
    private void CreateBackground(int mazeWidth, int mazeHeight)
    {
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.name = "Background";
        background.transform.SetParent(mazeParent);
        background.transform.position = new Vector3((mazeWidth - 1) / 2f, -0.5f, (mazeHeight - 1) / 2f); 
        background.transform.eulerAngles = new Vector3(90, 0, 0);
        background.transform.localScale = new Vector3(mazeWidth, mazeHeight, 1);
        
        var renderer = background.GetComponent<Renderer>();
        if (renderer != null && backgroundMaterial != null)
        {
            renderer.material = backgroundMaterial;
        }
        activeMazeObjects.Add(background);
    }

    private void PlaceStartAndEndPoints(Maze maze)
    {
        if (startPrefab == null || endPrefab == null) return;
        List<Vector2Int> edgeCells = GetEdgePathCells(maze);
        if (edgeCells.Count < 2) return;
        Vector2Int startPosition = edgeCells[Random.Range(0, edgeCells.Count)];
        Dictionary<Vector2Int, int> distances = CalculateDistances(maze, startPosition);
        Vector2Int endPosition = startPosition;
        int maxDistance = 0;
        foreach (var cell in edgeCells)
        {
            if (distances.TryGetValue(cell, out int distance) && distance > maxDistance)
            {
                maxDistance = distance;
                endPosition = cell;
            }
        }
        Vector3 startWorldPosition = new Vector3(startPosition.x, 0, startPosition.y);
        Vector3 endWorldPosition = new Vector3(endPosition.x, 0, endPosition.y);
        
        activeMazeObjects.Add(Instantiate(startPrefab, startWorldPosition, Quaternion.identity, mazeParent));
        activeMazeObjects.Add(Instantiate(endPrefab, endWorldPosition, Quaternion.identity, mazeParent));

        if (playerObject != null)
        {
            // Przesuwamy gracza, wyłączając i włączając CharacterController, aby nadpisać pozycję
            var controller = playerObject.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            playerObject.transform.position = startWorldPosition;
            if (controller != null) controller.enabled = true;
        }
    }
    
    private List<Vector2Int> GetEdgePathCells(Maze maze)
    {
        var edgeCells = new List<Vector2Int>();
        for (int x = 1; x < maze.Width - 1; x++)
        {
            if (maze.Grid[x, 1] == CellType.Path) edgeCells.Add(new Vector2Int(x, 1));
            if (maze.Grid[x, maze.Height - 2] == CellType.Path) edgeCells.Add(new Vector2Int(x, maze.Height - 2));
        }
        for (int y = 1; y < maze.Height - 1; y++)
        {
            if (maze.Grid[1, y] == CellType.Path) edgeCells.Add(new Vector2Int(1, y));
            if (maze.Grid[maze.Width - 2, y] == CellType.Path) edgeCells.Add(new Vector2Int(maze.Width - 2, y));
        }
        return edgeCells;
    }
    private Dictionary<Vector2Int, int> CalculateDistances(Maze maze, Vector2Int startNode)
    {
        var distances = new Dictionary<Vector2Int, int>();
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(startNode);
        distances[startNode] = 0;
        var directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            foreach (var dir in directions)
            {
                var neighbor = currentNode + dir;
                if (neighbor.x >= 0 && neighbor.x < maze.Width && neighbor.y >= 0 && neighbor.y < maze.Height &&
                    maze.Grid[neighbor.x, neighbor.y] == CellType.Path && !distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[currentNode] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return distances;
    }
}
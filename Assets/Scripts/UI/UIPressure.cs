using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UIPressure : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject goLeftFoot;

    private Tile[,] gridTiles;

    private int[,] resizedMatrix;
    private Dictionary<Vector2Int, GameObject> cubeMap;
    public int heightScale = 100;

    [Tooltip("Set value at 2^x+1. Ex: 33, 65, 129, ...")]
    public int resolution = 65;
    public GameObject go3DRightFoot;
    public GameObject goCubeTile;
    int[,] ReadMatrixFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int rowCount = lines.Length;
        int colCount = lines[0].Split(' ').Length;

        int[,] matrix = new int[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] values = lines[i].Split(' ');
            for (int j = 0; j < colCount; j++)
            {
                matrix[i, j] = int.Parse(values[j]);
            }
        }

        return matrix;
    }

    float[,] ReadDataFromFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        int rowCount = lines.Length;
        int colCount = lines[0].Split(' ').Length;

        float[,] heights = new float[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            string[] values = lines[i].Split(' ');
            for (int j = 0; j < colCount; j++)
            {
                heights[i, j] = float.Parse(values[j]);
            }
        }

        return heights;
    }


    private void Start()
    {
        OnSetUp();
        Generate3DGrid(); // Initial grid generation
        string filePath = Application.dataPath + "/Scripts/Grid/left_foot_matrix-1.txt"; // Path to the text file
        int[,] matrix = ReadMatrixFromFile(filePath);

        // Resize the original matrix to width x height
        resizedMatrix = ResizeMatrix(matrix, width, height);

        float[,] heights = getHeatMap();
        float maxHeight = FindMaxHeightValue(heights);
        NormalizeHeights(heights, maxHeight);

        // Initialize the 3D grid for the first time
        Generate3DGrid();
    }


    public void OnSetUp()
    {
        Debug.Log("UIPressure OnSetUp");
        gridTiles = new Tile[width, height];
        GenerateGrid();
        Generate3DGrid();
    }

    int[,] ResizeMatrix(int[,] originalMatrix, int newWidth, int newHeight)
    {
        int originalWidth = originalMatrix.GetLength(1);
        int originalHeight = originalMatrix.GetLength(0);

        int[,] resizedMatrix = new int[newHeight, newWidth];

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                float sourceX = x * (float)(originalWidth - 1) / (newWidth - 1);
                float sourceY = y * (float)(originalHeight - 1) / (newHeight - 1);

                int x0 = Mathf.FloorToInt(sourceX);
                int y0 = Mathf.FloorToInt(sourceY);
                int x1 = Mathf.CeilToInt(sourceX);
                int y1 = Mathf.CeilToInt(sourceY);

                float value = Mathf.Lerp(
                    Mathf.Lerp(originalMatrix[y0, x0], originalMatrix[y0, x1], sourceX - x0),
                    Mathf.Lerp(originalMatrix[y1, x0], originalMatrix[y1, x1], sourceX - x0),
                    sourceY - y0
                );

                resizedMatrix[y, x] = Mathf.RoundToInt(value);
            }
        }

        return resizedMatrix;
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab);
                spawnedTile.transform.SetParent(goLeftFoot.transform, false);
                spawnedTile.OnSetUp(x, y, this);

                RectTransform rectTransform = spawnedTile.GetComponent<RectTransform>();
                rectTransform.localScale = Vector3.one;
                rectTransform.anchoredPosition = new Vector2(spawnedTile.width * x, spawnedTile.height * y);

                spawnedTile.name = $"Tile {x} {y}";

                gridTiles[x, y] = spawnedTile;
            }
        }
    }
    private void Generate3DGrid()
    {
        cubeMap = new Dictionary<Vector2Int, GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cube = Instantiate(goCubeTile);
                cube.transform.SetParent(go3DRightFoot.transform, false);

                cube.transform.localPosition = new Vector3(x, 0, y);

                Tile tile = gridTiles[x, y];
                //float heightScale = Mathf.Clamp(tile.realValue / 100f, 0.1f, 5f);
                cube.transform.localScale = new Vector3(.1f, 1, 1);

                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material.color = tile.GetColorBasedOnValue(tile.realValue);

                // Store the cube in the dictionary with its grid position as the key
                cubeMap[new Vector2Int(x, y)] = cube;
            }
        }
    }

    // Method to update the 3D grid data dynamically
    public void Update3DGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = gridTiles[x, y];
                GameObject cube;

                // Check if cube exists in the map
                if (cubeMap.TryGetValue(new Vector2Int(x, y), out cube))
                {
                    // Update height based on realValue
                    float heightScale = Mathf.Clamp(tile.realValue / 100f*10f, 0.1f, 100f);
                    cube.transform.localScale = new Vector3(heightScale,1, 1);

                    // Update color based on the new value
                    Renderer renderer = cube.GetComponent<Renderer>();
                    renderer.material.color = tile.GetColorBasedOnValue(tile.realValue);
                }
            }
        }
    }

    public void Update3DGridTile(int x, int y)
    {
        Tile tile = gridTiles[x, y];
        GameObject cube;

        // Check if cube exists in the map
        if (cubeMap.TryGetValue(new Vector2Int(x, y), out cube))
        {
            // Update height based on realValue
            float heightScale = Mathf.Clamp(tile.realValue / 100f * 10f, 0.1f, 100f);
            cube.transform.localScale = new Vector3(heightScale, 1, 1);

            // Update color based on the new value
            Renderer renderer = cube.GetComponent<Renderer>();
            renderer.material.color = tile.GetColorBasedOnValue(tile.realValue);
        }
    }
    public void boundary()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tmpTile = gridTiles[x, y];
                tmpTile.realValue *= resizedMatrix[height - 1 - y, x];
                tmpTile.image.color = tmpTile.GetColorBasedOnValue(tmpTile.realValue);
                tmpTile.UpdateValue(tmpTile.realValue);
            }
        }
        float[,] heights = getHeatMap();
        float maxHeight = FindMaxHeightValue(heights);
        NormalizeHeights(heights, maxHeight);
    }

    public void boundaryTile(int x, int y)
    {
        Tile tmpTile = gridTiles[x, y];
        tmpTile.realValue *= resizedMatrix[height - 1 - y, x];
        tmpTile.image.color = tmpTile.GetColorBasedOnValue(tmpTile.realValue);
        tmpTile.UpdateValue(tmpTile.realValue);
    }

    public void UpdateAll()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                boundaryTile(x, y);
                Update3DGridTile(x, y);
            }
        }
    }

    public void ActOnNeighbors(int centerX, int centerY, int radius, int baseValue)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // use Manhattan distance to calculate the distance between the center and the current tile for an example
                int manhattanDistance = Mathf.Abs(x) + Mathf.Abs(y);
                if (manhattanDistance == 0)
                {
                    continue;
                }
                else if (manhattanDistance <= radius)
                {
                    int neighborX = centerX + x;
                    int neighborY = centerY + y;
                    if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                    {
                        Tile neighborTile = gridTiles[neighborX, neighborY];
                        int value = baseValue - (manhattanDistance * 1);
                        if (value < 0)
                        {
                            value = 0;
                        }

                        neighborTile.realValue += value;
                        // neighborTile.image.color = neighborTile.GetColorBasedOnValue(neighborTile.realValue);
                        neighborTile.UpdateValue(neighborTile.realValue);
                    }
                }
            }
        }
        UpdateAll();
    }

    public float[,] getHeatMap()
    {

        float[,] maps = new float[height, width];


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tmpTile = gridTiles[x, y];
                maps[y, x] = (float)tmpTile.realValue;
            }
        }

        return maps;
    }

    // Find the maximum height value in the matrix
    private float FindMaxHeightValue(float[,] matrix)
    {
        float max = float.MinValue;
        foreach (float height in matrix)
        {
            if (height > max)
            {
                max = height;
            }
        }
        return max;
    }

    private void NormalizeHeights(float[,] matrix, float maxHeight)
    {
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int z = 0; z < matrix.GetLength(1); z++)
            {
                matrix[x, z] /= maxHeight;
            }
        }
    }
}

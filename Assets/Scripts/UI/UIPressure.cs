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

    private void Start()
    {
        OnSetUp();
        string filePath = Application.dataPath + "/Scripts/Grid/left_foot_matrix.txt"; // Path to the text file
        int[,] matrix = ReadMatrixFromFile(filePath);

        // Resize the original matrix to width x height
        resizedMatrix = ResizeMatrix(matrix, width, height);
    }

    public void OnSetUp()
    {
        Debug.Log("UIPressure OnSetUp");
        gridTiles = new Tile[width, height];
        GenerateGrid();
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
    public void boundary()
    {
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tmpTile = gridTiles[x, y];
                tmpTile.realValue *= resizedMatrix[height-1-y, x];
                tmpTile.image.color = tmpTile.GetColorBasedOnValue(tmpTile.realValue);
                tmpTile.UpdateValue(tmpTile.realValue);
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
                        neighborTile.image.color = neighborTile.GetColorBasedOnValue(neighborTile.realValue);
                        neighborTile.UpdateValue(neighborTile.realValue);
                    }
                }
            }
        }
        boundary();
    }

}

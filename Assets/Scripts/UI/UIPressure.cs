using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using TMPro;
using System.Linq;

public class UIPressure : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject goLeftFoot;
    [SerializeField] private GameObject goRightFoot;

    public Tile[,] gridLeftTiles;
    public Tile[,] gridRightTiles;

    private int[,] resizedMatrix;
    private Dictionary<Vector2Int, GameObject> cubeMapL;
    private Dictionary<Vector2Int, GameObject> cubeMapR;
    public int heightScale = 100;

    [Tooltip("Set value at 2^x+1. Ex: 33, 65, 129, ...")]
    public int resolution = 65;

    public GameObject go3DRightFoot;
    public GameObject go3DLeftFoot;
    public GameObject goCubeTile;
    public TextMeshProUGUI textDebug;
    public TextAsset textFile;
    int[,] ReadMatrixFromText(string textContent)
    {
        string[] lines = textContent.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
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
    private void Awake()

    {
        OnSetUp();

        int[,] matrix = ReadMatrixFromText(textFile.text);
        Debug.Log("Matrix: " + matrix);

        // Resize the original matrix to width x height
        resizedMatrix = ResizeMatrix(matrix, width, height);
    }


    public void OnSetUp()
    {
        Debug.Log("UIPressure OnSetUp");
        gridLeftTiles = new Tile[width, height];
        gridRightTiles = new Tile[width, height];
        GenerateGrid();
        Generate3DLeftGrid();
        Generate3DRightGrid();
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
                var spawnedLTile = Instantiate(tilePrefab);
                spawnedLTile.transform.SetParent(goLeftFoot.transform, false);
                spawnedLTile.OnSetUp(x, y, this, 0);

                RectTransform rectLTransform = spawnedLTile.GetComponent<RectTransform>();
                rectLTransform.localScale = Vector3.one;
                rectLTransform.anchoredPosition = new Vector2(spawnedLTile.width * x, spawnedLTile.height * y);
                spawnedLTile.name = $"TileLeft {x} {y}";
                var spawnedRTile = Instantiate(tilePrefab);
                spawnedRTile.transform.SetParent(goRightFoot.transform, false);
                spawnedRTile.OnSetUp(x, y, this, 1);

                RectTransform rectRTransform = spawnedRTile.GetComponent<RectTransform>();
                rectRTransform.localScale = Vector3.one;
                rectRTransform.anchoredPosition = new Vector2(spawnedRTile.width * x, spawnedRTile.height * y);
                spawnedRTile.name = $"TileRight {x} {y}";

                gridLeftTiles[x, y] = spawnedLTile;
                gridRightTiles[x, y] = spawnedRTile;
            }
        }
    }

    public void ResetHeightGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tileL = gridLeftTiles[x, y];
                tileL.realValue = 0;
                tileL.image.color = Color.white;
                GameObject cubeL;

                // Check if cube exists in the map
                if (cubeMapL.TryGetValue(new Vector2Int(x, y), out cubeL))
                {

                    cubeL.transform.localScale = new Vector3(1, 1, 1);

                    // Update color based on the new value
                    Renderer renderer = cubeL.GetComponent<Renderer>();
                    renderer.material.color = Color.white;
                }

                Tile tileR = gridRightTiles[x, y];
                tileR.realValue = 0;
                tileR.image.color = Color.white;
                GameObject cubeR;

                // Check if cube exists in the map
                if (cubeMapR.TryGetValue(new Vector2Int(x, y), out cubeR))
                {
                    // Update height based on realValue

                    cubeR.transform.localScale = new Vector3(1, 1, 1);


                    Renderer renderer = cubeR.GetComponent<Renderer>();
                    renderer.material.color = Color.white;
                }
            }
        }
    }
    private void Generate3DLeftGrid()
    {
        if (cubeMapL != null)
        {
            foreach (var cube in cubeMapL.Values)
            {
                Destroy(cube);
            }
        }
        cubeMapL = new Dictionary<Vector2Int, GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cube = Instantiate(goCubeTile);
                cube.transform.SetParent(go3DLeftFoot.transform, false);
                cube.transform.localPosition = new Vector3(x, 0, y);
                Tile tile = gridLeftTiles[x, y];
                cube.transform.localScale = new Vector3(.1f, 1, 1);
                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material.color = tile.GetColorBasedOnValue(tile.realValue);
                cubeMapL[new Vector2Int(x, y)] = cube;
            }
        }
    }

    private void Generate3DRightGrid()
    {
        if (cubeMapR != null)
        {
            foreach (var cube in cubeMapR.Values)
            {
                Destroy(cube);
            }
        }
        cubeMapR = new Dictionary<Vector2Int, GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cube = Instantiate(goCubeTile);
                cube.transform.SetParent(go3DRightFoot.transform, false);
                cube.transform.localPosition = new Vector3(x, 0, y);
                Tile tile = gridRightTiles[x, y];
                cube.transform.localScale = new Vector3(.1f, 1, 1);
                Renderer renderer = cube.GetComponent<Renderer>();
                renderer.material.color = tile.GetColorBasedOnValue(tile.realValue);
                cubeMapR[new Vector2Int(x, y)] = cube;
            }
        }
    }


    public void Update3DGridTile(int x, int y)
    {
        Tile tileL = gridLeftTiles[x, y];
        GameObject cubeL;

        // Check if cube exists in the map
        if (cubeMapL.TryGetValue(new Vector2Int(x, y), out cubeL))
        {
            // Update height based on realValue
            float heightScale = Mathf.Clamp(tileL.realValue / 100f * 10f, 0.1f, 100f);
            cubeL.transform.localScale = new Vector3(heightScale, 1, 1);

            // Update color based on the new value
            Renderer renderer = cubeL.GetComponent<Renderer>();
            renderer.material.color = tileL.GetColorBasedOnValue(tileL.realValue);
        }

        Tile tileR = gridRightTiles[x, y];
        GameObject cubeR;

        // Check if cube exists in the map
        if (cubeMapR.TryGetValue(new Vector2Int(x, y), out cubeR))
        {
            // Update height based on realValue
            float heightScale = Mathf.Clamp(tileR.realValue / 100f * 10f, 0.1f, 100f);
            cubeR.transform.localScale = new Vector3(heightScale, 1, 1);

            // Update color based on the new value
            Renderer renderer = cubeR.GetComponent<Renderer>();
            renderer.material.color = tileR.GetColorBasedOnValue(tileR.realValue);
        }
    }

    public void boundaryTile(int x, int y, int side)
    {
        if (side == 1)
        {
            Tile tmpTile = gridRightTiles[x, y];
            tmpTile.realValue *= resizedMatrix[height - 1 - y, width - 1 - x];
            if (resizedMatrix[height - 1 - y, width - 1 - x] == 0)
            {
                GameObject cubeR;
                if (cubeMapR.TryGetValue(new Vector2Int(x, y), out cubeR))
                {
                    cubeR.gameObject.SetActive(false);
                }
            }
            tmpTile.image.color = tmpTile.GetColorBasedOnValue(tmpTile.realValue);
            tmpTile.UpdateValue(tmpTile.realValue);
        }
        else
        {
            Tile tmpTile = gridLeftTiles[x, y];
            tmpTile.realValue *= resizedMatrix[height - 1 - y, x];

            if (resizedMatrix[height - 1 - y, x] == 0)
            {
                GameObject cubeL;
                if (cubeMapL.TryGetValue(new Vector2Int(x, y), out cubeL))
                {
                    cubeL.gameObject.SetActive(false);
                }
            }
            tmpTile.image.color = tmpTile.GetColorBasedOnValue(tmpTile.realValue);
            tmpTile.UpdateValue(tmpTile.realValue);
        }

    }

    public void UpdateAll(int side)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                boundaryTile(x, y, side);
                Update3DGridTile(x, y);
            }
        }
    }

    public void ActOnNeighbors(int centerX, int centerY, int radius, int baseValue, int side)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
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
                        if (side == 0)
                        {
                            Tile neighborTile = gridLeftTiles[neighborX, neighborY];
                            int value = baseValue - (manhattanDistance * 1);
                            if (value < 0)
                            {
                                value = 0;
                            }

                            neighborTile.realValue += value;
                            // neighborTile.image.color = neighborTile.GetColorBasedOnValue(neighborTile.realValue);
                            neighborTile.UpdateValue(neighborTile.realValue);
                        }
                        else
                        {
                            Tile neighborTile = gridRightTiles[neighborX, neighborY];
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
        }

        UpdateAll(side);
    }
}
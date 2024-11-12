using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using TMPro;
using System.Linq;
using System.Drawing;

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
    public GameObject goCubeTileR;
    public TextMeshProUGUI textDebug;
    public TextAsset textFile;

    public Material instancedMaterial;
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
        if (instancedMaterial == null)
        {
            instancedMaterial = new Material(goCubeTile.GetComponent<Renderer>().sharedMaterial);
            instancedMaterial.enableInstancing = true;
        }
        Shader shader = Shader.Find("myShader");
        if (shader != null)
        {
            instancedMaterial.shader = shader;
            Debug.Log("Locate Shader");
        }
        else
        {
            Debug.Log("Shader not found!");
        }
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
                gridLeftTiles[x, y].realValue = 0;
                gridLeftTiles[x, y].GetColorBasedOnValue(0);

                gridRightTiles[x, y].realValue = 0;
                gridRightTiles[x, y].GetColorBasedOnValue(0);
            }
        }
    }

    // With GPU Instancing
    private void Generate3DLeftGrid()
    {
        Matrix4x4[] matrices = new Matrix4x4[width * height];
        Vector4[] colors = new Vector4[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;
                Vector3 localPosition = new Vector3(x, 0, y);
                Vector3 position = go3DLeftFoot.transform.position + go3DLeftFoot.transform.TransformVector(localPosition);

                Quaternion rotation = Quaternion.identity;
                Vector3 scale = new Vector3(1, 1, 1);
                matrices[index] = Matrix4x4.TRS(position, rotation, scale);
                colors[index] = gridLeftTiles[x, y].GetColorBasedOnValue(gridLeftTiles[x, y].realValue);
            }
        }

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetVectorArray("_Color", colors);

        Graphics.DrawMeshInstanced(goCubeTile.GetComponent<MeshFilter>().sharedMesh, 0, instancedMaterial, matrices, width * height, props);
    }

    // With GPU Instancing
    private void Generate3DRightGrid()
    {
        Matrix4x4[] matrices = new Matrix4x4[width * height];
        Vector4[] colors = new Vector4[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;
                Vector3 localPosition = new Vector3(x, 0, y);
                Vector3 position = go3DRightFoot.transform.position + go3DRightFoot.transform.TransformVector(localPosition);

                Quaternion rotation = Quaternion.identity;
                Vector3 scale = new Vector3(1, 1, 1);
                matrices[index] = Matrix4x4.TRS(position, rotation, scale);
                colors[index] = gridRightTiles[x, y].GetColorBasedOnValue(gridRightTiles[x, y].realValue);
            }
        }

        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetVectorArray("_Color", colors);

        Graphics.DrawMeshInstanced(goCubeTile.GetComponent<MeshFilter>().sharedMesh, 0, instancedMaterial, matrices, width * height, props);
    }

    public void Update3DGrid()
    {
        Matrix4x4[] matricesL = new Matrix4x4[width * height];
        Vector4[] colorsL = new Vector4[width * height];

        Matrix4x4[] matricesR = new Matrix4x4[width * height];
        Vector4[] colorsR = new Vector4[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x * height + y;

                Tile tileL = gridLeftTiles[x, y];
                float heightScaleL = Mathf.Clamp(tileL.realValue / 100f * 10f, 0.1f, 100f);


                Vector3 localPositionL = new Vector3(x, 0, y);
                Vector3 positionL = go3DLeftFoot.transform.position + go3DLeftFoot.transform.TransformVector(localPositionL);

                Quaternion rotationL = go3DLeftFoot.transform.rotation;
                Vector3 scaleL = new Vector3(1, heightScaleL, 1);
                matricesL[index] = Matrix4x4.TRS(positionL, rotationL, scaleL);
                colorsL[index] = gridLeftTiles[x, y].GetColorBasedOnValue(gridLeftTiles[x, y].realValue);

                Tile tileR = gridRightTiles[x, y];
                float heightScaleR = Mathf.Clamp(tileL.realValue / 100f * 10f, 0.1f, 100f);

                Vector3 localPositionR = new Vector3(x, 0, y);
                Vector3 positionR = go3DRightFoot.transform.position + go3DRightFoot.transform.TransformVector(localPositionR);

                Quaternion rotationR = go3DRightFoot.transform.rotation;
                Vector3 scaleR = new Vector3(1, heightScaleR, 1);
                matricesR[index] = Matrix4x4.TRS(positionR, rotationR, scaleR);
                colorsR[index] = gridRightTiles[x, y].GetColorBasedOnValue(gridRightTiles[x, y].realValue);
            }
        }

        MaterialPropertyBlock propsL = new MaterialPropertyBlock();
        propsL.SetVectorArray("_Color", colorsL);

        Graphics.DrawMeshInstanced(goCubeTile.GetComponent<MeshFilter>().sharedMesh, 0, instancedMaterial, matricesL, width * height, propsL);

        MaterialPropertyBlock propsR = new MaterialPropertyBlock();
        propsR.SetVectorArray("_Color", colorsR);

        Graphics.DrawMeshInstanced(goCubeTile.GetComponent<MeshFilter>().sharedMesh, 0, instancedMaterial, matricesR, width * height, propsR);
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
                            neighborTile.realValue *= resizedMatrix[height - 1 - neighborY, neighborX]; 
                            neighborTile.image.color = neighborTile.GetColorBasedOnValue(neighborTile.realValue);
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
                            neighborTile.realValue *= resizedMatrix[height - 1 - neighborY, width - 1 - neighborX];
                            neighborTile.image.color = neighborTile.GetColorBasedOnValue(neighborTile.realValue);
                            neighborTile.UpdateValue(neighborTile.realValue);

                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        Update3DGrid();
    }
}
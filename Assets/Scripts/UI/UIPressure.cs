using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPressure : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private GameObject goLeftFoot;

    private Tile[,] gridTiles;

    private void Start()
    {
        OnSetUp();
    }

    public void OnSetUp()
    {
        Debug.Log("UIPressure OnSetUp");
        gridTiles = new Tile[width, height];
        GenerateGrid();
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
    }

}

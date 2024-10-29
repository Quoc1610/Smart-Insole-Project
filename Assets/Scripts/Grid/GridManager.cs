using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Grid:MonoBehaviour
{

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Tile tilePrefab;
    
    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                spawnedTile.transform.SetParent(transform);
                spawnedTile.name = $"Tile {x} {y}";
            }
        }
    }
}

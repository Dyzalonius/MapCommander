using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public TerrainCell[,] cells;
    public Vector2Int Coordinate;
    private int cellSize = 1;
    private int chunkSize;

    public TerrainChunk(Vector2Int coordinate, int cellSize, int chunkSize)
    {
        Coordinate = coordinate;
        this.cellSize = cellSize;
        this.chunkSize = chunkSize;
    }

    private void CreateCells()
    {
        // Fill cells with TerrainCell objects
        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                TerrainCell newCell = new TerrainCell(new Vector2Int(Coordinate.x + i, Coordinate.y + j));
                cells[i, j] = newCell;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class TerrainGrid
{
    public TerrainChunk[,] chunks;
    private Vector2Int gridSizeInChunks = new Vector2Int(100, 100);
    private int cellSize = 1;
    private int chunkSize = 1000;

    public TerrainGrid(int width, int height, int cellSize, int chunkSize)
    {
        gridSizeInChunks = new Vector2Int(width, height);
        this.cellSize = cellSize;
        chunks = new TerrainChunk[gridSizeInChunks.x, gridSizeInChunks.y];

        CreateChunks();
    }

    private void CreateChunks()
    {
        // Fill chunks with TerrainChunk objects
        for (int i = 0; i < gridSizeInChunks.x; i++)
        {
            for (int j = 0; j < gridSizeInChunks.y; j++)
            {
                TerrainChunk newChunk = new TerrainChunk(new Vector2Int(i * chunkSize, j * chunkSize), cellSize, chunkSize);
                chunks[i,j] = newChunk;
            }
        }
    }
}

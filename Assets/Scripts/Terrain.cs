using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private Vector2Int gridSizeInChunks;

    [SerializeField]
    private int cellSize;

    [SerializeField]
    private int chunkSize;

    [SerializeField]
    private Material groundMaterial;

    [HideInInspector]
    public TerrainChunk[,] chunks;

    private Mesh groundMesh;

    private void Start()
    {
        chunks = new TerrainChunk[gridSizeInChunks.x, gridSizeInChunks.y];
        CreateChunks();
        groundMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = groundMesh;
        float width = gridSizeInChunks.x * chunkSize * cellSize;
        float height = gridSizeInChunks.y * chunkSize * cellSize;
        groundMesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(width, 0, 0), new Vector3(0, 0, height), new Vector3(width, 0, height) };
        groundMesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        groundMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 1 };
    }

    private void Update()
    {
        Graphics.DrawMesh(groundMesh, Vector3.zero, Quaternion.identity, groundMaterial, 0);
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

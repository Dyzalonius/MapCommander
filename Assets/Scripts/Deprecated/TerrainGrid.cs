using System.Collections.Generic;
using UnityEngine;

public class TerrainGrid : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    public Vector2Int GridSizeInChunks; // Amount of chunks in x and y direction

    [SerializeField]
    public int CellSize; // Width and height of one cell

    [SerializeField]
    public int ChunkSize; // Cells per chunk

    [SerializeField]
    private int forestCameraZoomThreshold;

    [Header("References")]
    [SerializeField]
    private Material groundMaterial;

    [SerializeField]
    public Material ForestMaterial;

    [SerializeField]
    private Camera cam;

    [HideInInspector]
    public TerrainChunk[,] chunks;

    [HideInInspector]
    public List<TerrainChunk> activeChunks = new List<TerrainChunk>();

    [HideInInspector]
    public static TerrainGrid Instance { get; private set; } // static singleton

    private Mesh groundMesh;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        chunks = new TerrainChunk[GridSizeInChunks.x, GridSizeInChunks.y];
        CreateChunks();

        groundMesh = new Mesh();
        float width = GridSizeInChunks.x * ChunkSize * CellSize;
        float height = GridSizeInChunks.y * ChunkSize * CellSize;
        groundMesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(width, 0, 0), new Vector3(0, 0, height), new Vector3(width, 0, height) };
        groundMesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        groundMesh.triangles = new int[6] { 0, 1, 2, 2, 3, 1 };
    }

    private void Update()
    {
        // Draw ground
        Graphics.DrawMesh(groundMesh, Vector3.zero, Quaternion.identity, groundMaterial, 8);

        // Draw forests if camera is zoomed in far enough
        /*UpdateActiveChunks();
        if (cam.orthographicSize < forestCameraZoomThreshold)
        {
            foreach (TerrainChunk chunk in activeChunks)
            {
                chunk.DrawForest();
            }
        }*/
    }

    private void CreateChunks()
    {
        // Fill chunks with TerrainChunk objects
        for (int i = 0; i < GridSizeInChunks.x; i++)
        {
            for (int j = 0; j < GridSizeInChunks.y; j++)
            {
                TerrainChunk newChunk = new TerrainChunk(new Vector2Int(i * ChunkSize, j * ChunkSize), CellSize, ChunkSize);
                chunks[i,j] = newChunk;
            }
        }
    }

    private void UpdateActiveChunks()
    {
        foreach (TerrainChunk chunk in chunks)
        {
            if (!activeChunks.Contains(chunk))
                activeChunks.Add(chunk);
        }
    }
}

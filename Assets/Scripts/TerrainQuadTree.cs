using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainQuadTree : MonoBehaviour
{
    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private QuadrantSize minSize;

    [SerializeField]
    private QuadrantSize maxSize;

    [SerializeField]
    private TerrainTexture2 terrainPlanePrefab;

    private Quadrant root;
    private List<Quadrant> activeQuadrants;
    private Dictionary<TerrainTexture2, Quadrant> quadrantPairs;

    private void Start()
    {
        BuildTree();
        GenerateTerrain();
        activeQuadrants = new List<Quadrant> { root };
        quadrantPairs = new Dictionary<TerrainTexture2, Quadrant>();
    }

    private void BuildTree()
    {
        root = new Quadrant(maxSize, minSize, Vector2Int.zero, position);
    }

    private void GenerateTerrain()
    {
        StartCoroutine(Threaded.RunOnThread(CreateTerrainData, DrawTerrain));
    }

    private void CreateTerrainData()
    {
        root.GenerateNoiseMap();
    }

    private void DrawTerrain()
    {
        foreach (Quadrant quadrant in activeQuadrants)
        {
            TerrainTexture2 newTerrainTexture = Instantiate(terrainPlanePrefab, transform);
            newTerrainTexture.Setup(quadrant);
            quadrantPairs.Add(newTerrainTexture, quadrant);
        }
    }

    void OnDrawGizmos()
    {
        List<Quadrant> quadrants = root.GetQuadrants();
        quadrants.ForEach(x => DrawGizmoThing(x.position, x.size));
    }

    private void DrawGizmoThing(Vector2Int position, Vector2Int size)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(position.x + size.x / 2, 0f, position.y + size.y / 2), new Vector3(size.x, 0f, size.y));
    }
}

public enum QuadrantSize
{
    k4 = 4096,
    k8 = 8192,
    k16 = 16384,
    k32 = 32768,
    k64 = 65536,
    k128 = 131072
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
    private QuadrantSize activeQuadrantScale;

    private void Start()
    {
        BuildTree();
        GenerateTerrain();
        activeQuadrants = new List<Quadrant> { root };
        quadrantPairs = new Dictionary<TerrainTexture2, Quadrant>();
        activeQuadrantScale = maxSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            activeQuadrantScale = QuadrantSize.k4;
            List<Quadrant> newActive = new List<Quadrant>();
            newActive.AddRange(root.GetQuadrants(false));

            foreach (var pair in quadrantPairs)
                Destroy(pair.Key.gameObject);

            activeQuadrants = newActive;
            foreach (Quadrant quadrant in activeQuadrants)
            {
                TerrainTexture2 newTerrainTexture = Instantiate(terrainPlanePrefab, transform);
                newTerrainTexture.Setup(quadrant);
                quadrantPairs.Add(newTerrainTexture, quadrant);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            List<Quadrant> leaves = root.GetLeafQuadrants();

            foreach (Quadrant quadrant in leaves)
                quadrant.SaveTexture();
        }

        float pos1 = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float pos2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.scaledPixelWidth, 0f, 0f)).x;
        float metersPerPixel = (pos2 - pos1) / Camera.main.scaledPixelWidth;
        float texelsPerMeter = (int)QuadrantSize.k4 / ((float)activeQuadrantScale);
        float texelsPerPixel = metersPerPixel * texelsPerMeter;
        Debug.Log("mpp = " + metersPerPixel + ", tpm = " + texelsPerMeter + ", tpp = " + texelsPerPixel);

        // Decrease detail when zooming out
        if (texelsPerPixel >= 2f && activeQuadrantScale != maxSize)
        {
            activeQuadrantScale = NextSize(activeQuadrantScale, true);
            UpdateActiveQuadrants();
        }

        // Increase detail when zooming in
        if (texelsPerPixel < 1f && activeQuadrantScale != minSize)
        {
            activeQuadrantScale = NextSize(activeQuadrantScale, false);
            UpdateActiveQuadrants();
        }
    }

    private void UpdateActiveQuadrants()
    {
        List<Quadrant> newActive = new List<Quadrant>();
        newActive.AddRange(root.FindQuadrantsOfSize(activeQuadrantScale));

        foreach (var pair in quadrantPairs)
            Destroy(pair.Key.gameObject);

        quadrantPairs.Clear();
        activeQuadrants = newActive;
        foreach (Quadrant quadrant in activeQuadrants)
        {
            TerrainTexture2 newTerrainTexture = Instantiate(terrainPlanePrefab, transform);
            newTerrainTexture.Setup(quadrant);
            quadrantPairs.Add(newTerrainTexture, quadrant);
        }
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
        root.GenerateTerrainData();
    }

    private void DrawTerrain()
    {
        root.GenerateTerrainTexture();
        UpdateActiveQuadrants();
    }

    void OnDrawGizmos()
    {
        List<Quadrant> quadrants = root.GetQuadrants(true);
        quadrants.ForEach(x => DrawGizmoThing(x.position, x.size));
    }

    private void DrawGizmoThing(Vector2Int position, Vector2Int size)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(position.x + size.x / 2, 0f, position.y + size.y / 2), new Vector3(size.x, 0f, size.y));
    }

    // Grab next size from QuadrantSize enum
    private QuadrantSize NextSize(QuadrantSize size, bool nextRatherThanPrevious = true)
    {
        QuadrantSize[] list = (QuadrantSize[])Enum.GetValues(typeof(QuadrantSize));
        if (nextRatherThanPrevious)
            return list[Array.IndexOf<QuadrantSize>(list, size) + 1];
        else
            return list[Array.IndexOf<QuadrantSize>(list, size) - 1];
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
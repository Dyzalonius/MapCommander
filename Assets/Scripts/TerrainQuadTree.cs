using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TerrainQuadTree : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private QuadrantSize minSize;

    [SerializeField]
    private QuadrantSize maxSize;

    [Header("References")]
    [SerializeField]
    private TerrainTexture terrainPlanePrefab;

    [HideInInspector]
    public static TerrainQuadTree Instance { get; private set; } // static singleton

    private Quadrant root;
    private List<Quadrant> activeQuadrants;
    private Dictionary<TerrainTexture, Quadrant> activeQuadrantPairs;
    private QuadrantSize activeQuadrantScale;

    private void Start()
    {
        activeQuadrants = new List<Quadrant>();
        activeQuadrantPairs = new Dictionary<TerrainTexture, Quadrant>();
        activeQuadrantScale = maxSize;
        BuildTree();
        GenerateTerrain();
    }

    public void BuildTree()
    {
        root = new Quadrant(maxSize, minSize, Vector2Int.zero, position);
    }

    private void Update()
    {
        UpdateTerrainScale();

        if (Input.GetKeyDown(KeyCode.S))
            SaveTerrain();
    }

    private void UpdateTerrainScale()
    {
        float pos1 = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float pos2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.scaledPixelWidth, 0f, 0f)).x;
        float metersPerPixel = (pos2 - pos1) / Camera.main.scaledPixelWidth;
        float texelsPerMeter = (int)QuadrantSize.k4 / ((float)activeQuadrantScale);
        float texelsPerPixel = metersPerPixel * texelsPerMeter;
        //Debug.Log("tpp = " + texelsPerPixel);

        // Decrease activeQuadrantScale when zooming out
        if (texelsPerPixel >= 2f && activeQuadrantScale != maxSize)
            activeQuadrantScale = NextSize(activeQuadrantScale, true);

        // Increase activeQuadrantScale when zooming in
        if (texelsPerPixel < 1f && activeQuadrantScale != minSize)
            activeQuadrantScale = NextSize(activeQuadrantScale, false);

        // Update activeQuadrants if a different set
        List<Quadrant> newActiveQuadrants = new List<Quadrant>();
        foreach (Quadrant quadrant in root.FindQuadrantsOfSize(activeQuadrantScale))
            if (quadrant.VisibleByMainCam())
                newActiveQuadrants.Add(quadrant);

        if (!newActiveQuadrants.Equals(activeQuadrants))
        {
            activeQuadrants = newActiveQuadrants;
            UpdateActiveQuadrants();
        }
    }

    private void UpdateActiveQuadrants() //TODO: optimize: Only delete quadrants that are now inactive, and create quadrants that we're inactive, and pool the TerrainTexture objects
    {
        // Delete old activeQuadrants
        foreach (var pair in activeQuadrantPairs)
            Destroy(pair.Key.gameObject);
        activeQuadrantPairs.Clear();

        // Create new activeQuadrants
        foreach (Quadrant quadrant in activeQuadrants)
        {
            TerrainTexture newTerrainTexture = Instantiate(terrainPlanePrefab, transform);
            newTerrainTexture.Setup(quadrant);
            activeQuadrantPairs.Add(newTerrainTexture, quadrant);
        }
    }

    private void SaveTerrain()
    {
        List<Quadrant> leaves = root.GetLeafQuadrants();

        foreach (Quadrant quadrant in leaves)
            quadrant.SaveTexture();
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
    }

    void OnDrawGizmos()
    {
        if (activeQuadrantPairs == null) return;

        foreach (var pair in activeQuadrantPairs)
        {
            DrawGizmoThing(pair.Value.position, pair.Value.size);
        }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

public class TerrainQuadTree : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private Vector2Int position;

    [SerializeField]
    private QuadrantSize minSize;

    [SerializeField]
    private QuadrantSize maxSize;

    [SerializeField]
    private string saveFilePath;

    [SerializeField]
    private bool loadFromSave;

    [SerializeField]
    public bool GenerateMapData;

    [Header("References")]
    [SerializeField]
    private TerrainTexture terrainPlanePrefab;

    [HideInInspector]
    public static TerrainQuadTree Instance { get; private set; } // static singleton

    private Quadrant root;
    private List<Quadrant> visibleQuadrants;
    private List<TerrainTexture> terrainTexturePool;
    private Dictionary<Quadrant, TerrainTexture> visibleQuadrantPairs;
    private QuadrantSize visibleQuadrantScale;

    private void Start()
    {
        visibleQuadrants = new List<Quadrant>();
        terrainTexturePool = new List<TerrainTexture>();
        visibleQuadrantPairs = new Dictionary<Quadrant, TerrainTexture>();
        visibleQuadrantScale = maxSize;
        BuildTree();
    }

    private void Update()
    {
        UpdateVisibleQuadrantScale();
        UpdateVisibleQuadrants();

        if (Input.GetKeyDown(KeyCode.S))
            SaveTerrain();
    }

    public void BuildTree()
    {
        root = new Quadrant(maxSize, minSize, position);
        StartCoroutine(Threaded.RunOnThread(CreateTerrainData, DrawTerrain));
    }

    private void UpdateVisibleQuadrantScale()
    {
        float pos1 = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
        float pos2 = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.scaledPixelWidth, 0f, 0f)).x;
        float metersPerPixel = (pos2 - pos1) / Camera.main.scaledPixelWidth;
        float texelsPerMeter = (int)QuadrantSize.k4 / ((float)visibleQuadrantScale);
        float texelsPerPixel = metersPerPixel * texelsPerMeter;
        //Debug.Log("tpp = " + texelsPerPixel);

        // Decrease activeQuadrantScale when zooming out
        if (texelsPerPixel >= 2f && visibleQuadrantScale != maxSize)
            visibleQuadrantScale = NextSize(visibleQuadrantScale, true);

        // Increase activeQuadrantScale when zooming in
        if (texelsPerPixel < 1f && visibleQuadrantScale != minSize)
            visibleQuadrantScale = NextSize(visibleQuadrantScale, false);
    }

    private void UpdateVisibleQuadrants()
    {
        // Find quadrants that have to be visible
        List<Quadrant> newActiveQuadrants = new List<Quadrant>();
        foreach (Quadrant quadrant in root.FindQuadrantsOfSize(visibleQuadrantScale))
            if (quadrant.VisibleByMainCam())
                newActiveQuadrants.Add(quadrant);

        // Deactivate visible quadrants that have to be invisible
        for (int i = visibleQuadrants.Count - 1; i >= 0; i--)
        {
            Quadrant quadrant = visibleQuadrants[i];
            if (!newActiveQuadrants.Contains(quadrant))
            {
                TerrainTexture terrainTexture = visibleQuadrantPairs[quadrant];
                terrainTexture.Deactivate();
                visibleQuadrantPairs.Remove(quadrant);
                terrainTexturePool.Add(terrainTexture);
            }
        }

        // Activate new visible quadrants
        visibleQuadrants = newActiveQuadrants;
        foreach (Quadrant quadrant in visibleQuadrants)
            if (!visibleQuadrantPairs.ContainsKey(quadrant))
            {
                // Only instantiate a new terrainTexture if the pool is empty
                if (terrainTexturePool.Count == 0)
                    terrainTexturePool.Add(Instantiate(terrainPlanePrefab, transform));

                TerrainTexture terrainTexture = terrainTexturePool[0];
                terrainTexturePool.Remove(terrainTexture);
                terrainTexture.gameObject.SetActive(true);
                terrainTexture.Setup(quadrant, GenerateMapData);
                visibleQuadrantPairs.Add(quadrant, terrainTexture);
            }
    }

    private void SaveTerrain()
    {
        Debug.Log("start save");
        List<Quadrant> leaves = root.GetLeafQuadrants();

        //foreach (Quadrant quadrant in leaves)
        //    quadrant.SaveTexture();

        root.SaveTexture();

        Debug.Log("finish save");
    }

    private void LoadTerrain()
    {
        root.LoadTerrain(saveFilePath);
    }

    private void CreateTerrainData()
    {
        root.GenerateTerrainData(GenerateMapData);
    }

    private void DrawTerrain()
    {
        root.GenerateTerrainTexture(GenerateMapData);
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

    void OnDrawGizmos()
    {
        if (visibleQuadrantPairs == null) return;

        foreach (var pair in visibleQuadrantPairs)
        {
            Vector2Int position = pair.Key.position;
            Vector2Int size = pair.Key.size;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(new Vector3(position.x + size.x / 2, 0f, position.y + size.y / 2), new Vector3(size.x, 0f, size.y));
        }
        Gizmos.DrawWireCube(new Vector3(root.position.x + root.size.x / 2, 0f, root.position.y + root.size.y / 2), new Vector3(root.size.x, 0f, root.size.y));
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private Vector2Int terrainSize; // Width and height of entire terrain

    [SerializeField]
    private int cellSize; // Width and height of one cell

    [SerializeField]
    private float noiseScale;

    [SerializeField]
    private int octaves; // Amount of overlapping noise maps used

    [SerializeField]
    [Range(0, 1)]
    private float persistance; // Ratio for amplitudes of each octave

    [SerializeField]
    private float lacunarity; // Ratio for frequencies of each octave

    [SerializeField]
    private int seed; // Seed for noisemaps

    [SerializeField]
    private Vector2 offset; // Offset of noiseMap

    [SerializeField]
    public bool AutoGenerate; // Auto generate when values change

    [Header("References")]
    [SerializeField]
    private TerrainTexture terrain;

    private float[,] noiseMap;

    private void Start()
    {
        //Generate();
    }

    public void Generate()
    {
        StartCoroutine(Threaded.RunOnThread(GenerateNoiseMap, DrawNoiseMap));
    }

    private void GenerateNoiseMap()
    {
        noiseMap = Noise.GenerateNoiseMap(terrainSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
    }

    private void DrawNoiseMap()
    {
        terrain.DrawNoiseMap(noiseMap);
    }

    private void OnValidate()
    {
        if (terrainSize.x < 1)
            terrainSize.x = 1;

        if (terrainSize.y < 1)
            terrainSize.y = 1;

        if (octaves < 1)
            octaves = 1;

        if (lacunarity < 1)
            lacunarity = 1;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(Vector2Int size, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        // Clamp scale
        if (scale <= 0f)
            scale = 0.0001f;

        System.Random pseudoRandomNumberGenerator = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = pseudoRandomNumberGenerator.Next(-100000, 100000) + offset.x;
            float offsetY = pseudoRandomNumberGenerator.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }


        // Generate noise map
        float[,] noiseMap = new float[size.x, size.y];
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                // Loop through octaves
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Value between -1 and +1

                    // Change noiseHeight
                    noiseHeight += perlinValue * amplitude;

                    // Update amplitude and frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Update min max noise height
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                // Set noise height
                noiseMap[x, y] = noiseHeight;
            }

        // Normalise all values in the noiseMap to values between 0 and 1
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }

                return noiseMap;
    }
}

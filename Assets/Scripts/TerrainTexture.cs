using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTexture : MonoBehaviour
{
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = new Color(noiseMap[x, y], 0f, 0f);
            }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width / 10, 1, height / 10);
        textureRenderer.transform.localPosition = new Vector3(width / 2, 1, height / 2);
    }
}

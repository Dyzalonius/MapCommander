using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTexture : MonoBehaviour
{
    [SerializeField]
    private Renderer textureRenderer;

    private Vector2Int size;
    private Color[] colorMap;
    private float[,] noiseMap;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        this.noiseMap = noiseMap;
        size.Set(noiseMap.GetLength(0), noiseMap.GetLength(1));

        StartCoroutine(Threaded.RunOnThread(GetMapData, DrawMap));
    }

    private void GetMapData()
    {
        colorMap = new Color[size.x * size.y];
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                colorMap[y * size.x + x] = new Color(noiseMap[x, y], 0f, 0f);
            }
    }

    private void DrawMap()
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(size.x / 10, 1, size.y / 10);
        textureRenderer.transform.localPosition = new Vector3(size.x / 2, 1, size.y / 2);
    }
}

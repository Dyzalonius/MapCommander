using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Quadrant
{
    public Vector2Int position;
    public Vector2Int size; // Size of the quadrant, not size of the texture
    private Quadrant parent;
    private int depth;

    private Quadrant topLeft;
    private Quadrant topRight;
    private Quadrant bottomLeft;
    private Quadrant bottomRight;

    public Texture2D texture; // Map data texture
    public int textureSize = (int)QuadrantSize.k4;

    private float[,] noiseMap;
    private Color[] colorMap;

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int position)
    {
        this.position = position;
        this.size = new Vector2Int((int)size, (int)size);
        Debug.Log("Size = " + this.size);

        if ((int)size > (int)minSize)
            Split(NextSize(size, false), minSize);
    }

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int offset, Quadrant parent)
    {
        this.parent = parent;
        position = new Vector2Int(this.parent.position.x + (int)size * offset.x, this.parent.position.y + (int)size * offset.y);
        this.size = new Vector2Int((int)size, (int)size);

        if ((int)size > (int)minSize)
            Split(NextSize(size, false), minSize);
    }

    // Create four new Quadrants inside current Quadrant
    private void Split(QuadrantSize size, QuadrantSize minSize)
    {
        topLeft = new Quadrant(size, minSize, Vector2Int.up, this);
        topRight = new Quadrant(size, minSize, Vector2Int.one, this);
        bottomLeft = new Quadrant(size, minSize, Vector2Int.zero, this);
        bottomRight = new Quadrant(size, minSize, Vector2Int.right, this);
    }

    public List<Quadrant> GetQuadrants(bool recursive)
    {
        List<Quadrant> quadrants = new List<Quadrant>();

        if (recursive)
        {
            quadrants.Add(this);
            if (topLeft != null)
                quadrants.AddRange(topLeft.GetQuadrants(recursive));
            if (topRight != null)
                quadrants.AddRange(topRight.GetQuadrants(recursive));
            if (bottomLeft != null)
                quadrants.AddRange(bottomLeft.GetQuadrants(recursive));
            if (bottomRight != null)
                quadrants.AddRange(bottomRight.GetQuadrants(recursive));
        }
        else
        {
            quadrants.Add(topLeft);
            quadrants.Add(topRight);
            quadrants.Add(bottomLeft);
            quadrants.Add(bottomRight);
        }

        return quadrants;
    }

    public List<Quadrant> GetLeafQuadrants()
    {
        List<Quadrant> leaves = new List<Quadrant>();

        // If quadrant has children, add leafquadrants of children
        if (topLeft != null)
            leaves.AddRange(topLeft.GetLeafQuadrants());
        if (topRight != null)
            leaves.AddRange(topRight.GetLeafQuadrants());
        if (bottomLeft != null)
            leaves.AddRange(bottomLeft.GetLeafQuadrants());
        if (bottomRight != null)
            leaves.AddRange(bottomRight.GetLeafQuadrants());

        // If quadrant has no children, this quadrant is a leaf
        if (leaves.Count == 0)
            leaves.Add(this);

        return leaves;
    }

    public List<Quadrant> FindQuadrantsOfSize(QuadrantSize size)
    {
        List<Quadrant> quadrantsOfSize = new List<Quadrant>();

        if (this.size.x == (int)size)
            quadrantsOfSize.Add(this);
        else
        {
            if (topLeft != null)
                quadrantsOfSize.AddRange(topLeft.FindQuadrantsOfSize(size));
            if (topRight != null)
                quadrantsOfSize.AddRange(topRight.FindQuadrantsOfSize(size));
            if (bottomLeft != null)
                quadrantsOfSize.AddRange(bottomLeft.FindQuadrantsOfSize(size));
            if (bottomRight != null)
                quadrantsOfSize.AddRange(bottomRight.FindQuadrantsOfSize(size));
        }

        return quadrantsOfSize;
    }

    public void GenerateTerrainData(bool generate)
    {
        // Create noise map
        if (generate)
        {
            noiseMap = Noise.GenerateNoiseMap(new Vector2Int(textureSize, textureSize), 0, 300, 1, 0.5f, 1, Vector2.zero);

            // Create color map
            colorMap = new Color[noiseMap.Length];
            for (int y = 0; y < textureSize; y++)
                for (int x = 0; x < textureSize; x++)
                {
                    if (x * y > noiseMap.Length)
                        Debug.Log(x + " * " + y + " = " + (x * y));
                    colorMap[y * textureSize + x] = new Color(noiseMap[x, y], 0f, 0f);
                }
        }

        if (topLeft != null)
            topLeft.GenerateTerrainData(generate);
        if (topRight != null)
            topRight.GenerateTerrainData(generate);
        if (bottomLeft != null)
            bottomLeft.GenerateTerrainData(generate);
        if (bottomRight != null)
            bottomRight.GenerateTerrainData(generate);
        Debug.Log("generated");
    }

    public void GenerateTerrainTexture(bool generate)
    {
        if (generate)
        {
            texture = new Texture2D(textureSize, textureSize)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(colorMap);
            texture.Apply();
        }

        if (topLeft != null)
            topLeft.GenerateTerrainTexture(generate);
        if (topRight != null)
            topRight.GenerateTerrainTexture(generate);
        if (bottomLeft != null)
            bottomLeft.GenerateTerrainTexture(generate);
        if (bottomRight != null)
            bottomRight.GenerateTerrainTexture(generate);

        Debug.Log("applied");
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

    public void SaveTexture()
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
    }

    public bool VisibleByMainCam()
    {
        Vector3 bottomLeftPos = Camera.main.WorldToViewportPoint(new Vector3(position.x, 0f, position.y));
        Vector3 topRightPos = Camera.main.WorldToViewportPoint(new Vector3(position.x + size.x, 0f, position.y + size.y));

        return !(bottomLeftPos.x > 1f || topRightPos.x < 0f || bottomLeftPos.y > 1f || topRightPos.y < 0f);
    }
}
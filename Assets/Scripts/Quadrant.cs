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

    public void GenerateTerrainData()
    {
        // Create noise map
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

        if (topLeft != null)
            topLeft.GenerateTerrainData();
        if (topRight != null)
            topRight.GenerateTerrainData();
        if (bottomLeft != null)
            bottomLeft.GenerateTerrainData();
        if (bottomRight != null)
            bottomRight.GenerateTerrainData();
    }

    public void GenerateTerrainTexture()
    {
        texture = new Texture2D(textureSize, textureSize)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixels(colorMap);
        texture.Apply();

        if (topLeft != null)
            topLeft.GenerateTerrainTexture();
        if (topRight != null)
            topRight.GenerateTerrainTexture();
        if (bottomLeft != null)
            bottomLeft.GenerateTerrainTexture();
        if (bottomRight != null)
            bottomRight.GenerateTerrainTexture();
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

    public void SaveTerrainToPNG(string folderName, string suffix = "")
    {
        Debug.Log("save file");
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../TERRAINDATA/" + folderName + "/";

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        File.WriteAllBytes(dirPath + "Quadrant" + suffix + ".png", bytes);

        if (suffix == "")
            suffix += "_";

        if (topLeft != null)
            topLeft.SaveTerrainToPNG(folderName, suffix + "0");
        if (topRight != null)
            topRight.SaveTerrainToPNG(folderName, suffix + "1");
        if (bottomLeft != null)
            bottomLeft.SaveTerrainToPNG(folderName, suffix + "2");
        if (bottomRight != null)
            bottomRight.SaveTerrainToPNG(folderName, suffix + "3");
    }

    public void LoadTerrainFromPNG(string filePathBase, string suffix = "")
    {
        Texture2D texture = null;
        byte[] fileData;
        string filePath = Application.dataPath + filePathBase + "Quadrant" + suffix + ".png";

        if (!File.Exists(filePath))
        {
            Debug.Log(filePath);
            Debug.LogError("filePath does not exist");
            return;
        }

        fileData = File.ReadAllBytes(filePath);
        texture = new Texture2D(2, 2); // Dimensions don't matter, LoadImage auto-resizes
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadImage(fileData);

        this.texture = texture;

        if (suffix == "")
            suffix += "_";

        if (topLeft != null)
            topLeft.LoadTerrainFromPNG(filePathBase, suffix + "0");
        if (topRight != null)
            topRight.LoadTerrainFromPNG(filePathBase, suffix + "1");
        if (bottomLeft != null)
            bottomLeft.LoadTerrainFromPNG(filePathBase, suffix + "2");
        if (bottomRight != null)
            bottomRight.LoadTerrainFromPNG(filePathBase, suffix + "3");
    }

    public bool VisibleByMainCam()
    {
        Vector3 bottomLeftPos = Camera.main.WorldToViewportPoint(new Vector3(position.x, 0f, position.y));
        Vector3 topRightPos = Camera.main.WorldToViewportPoint(new Vector3(position.x + size.x, 0f, position.y + size.y));

        return !(bottomLeftPos.x > 1f || topRightPos.x < 0f || bottomLeftPos.y > 1f || topRightPos.y < 0f);
    }

    // return the Vector2Int position on this quadrant of a given world position. If position is off this quadrant, return (-1,-1)
    public Vector2Int PositionOnQuadrant(Vector2Int pos)
    {
        Vector2Int posOnQuadrant = new Vector2Int(-1, -1);

        if (pos.x >= position.x && pos.x < position.x + size.x && pos.y >= position.y && pos.y < position.y + size.y)
            posOnQuadrant = new Vector2Int(pos.x - position.x, pos.y - position.y);

        return posOnQuadrant;
    }

    public void Paint(Vector2Int position, TerrainBrushMode mode)
    {
        Color pixelColor = texture.GetPixel(size.x - position.x - 1, size.y - position.y - 1); //Dirty fix: For some reason the terrain is 180* rotated, so had to grab an odd pixel location
        Color pixelColorNew = pixelColor;

        switch (mode)
        {
            case TerrainBrushMode.ADDFOREST:
                pixelColorNew.g = 1f;
                break;

            case TerrainBrushMode.REMOVEFOREST:
                pixelColorNew.g = 0f;
                break;

            default:
                break;
        }

        // Edit texture if new color is different
        if (pixelColor != pixelColorNew)
        {
            texture.SetPixel(size.x - position.x - 1, size.y - position.y - 1, pixelColorNew); //Dirty fix: For some reason the terrain is 180* rotated, so had to grab an odd pixel location
            texture.Apply();
            TerrainQuadTree.Instance.UpdateTerrainTexture(this); //TODO: Make this also update textures of higher levels!
        }
    }
}
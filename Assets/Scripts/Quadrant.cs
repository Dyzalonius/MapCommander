using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Quadrant
{
    public Vector2Int position;
    public Vector2Int size; // Size of the quadrant, not size of the texture
    public string id;
    private Quadrant parent;
    private Quadrant bottomLeft;
    private Quadrant bottomRight;
    private Quadrant topLeft;
    private Quadrant topRight;
    public Texture2D texture; // Map data texture
    public int textureSize;

    private float[,] noiseMap;
    public Color[] colorMap;
    private Dictionary<Vector2Int, Color> terrainChanges = new Dictionary<Vector2Int, Color>();

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int position)
    {
        this.position = position;
        this.size = new Vector2Int((int)size, (int)size);
        textureSize = (int)minSize;
        Debug.Log("Size = " + this.size);
        id = "";

        if ((int)size > (int)minSize)
            Split(NextSize(size, false), minSize);
    }

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int offset, Quadrant parent, string idSuffix)
    {
        this.parent = parent;
        position = new Vector2Int(this.parent.position.x + (int)size * offset.x, this.parent.position.y + (int)size * offset.y);
        this.size = new Vector2Int((int)size, (int)size);
        textureSize = (int)minSize;
        id = parent.id + idSuffix;

        if ((int)size > (int)minSize)
            Split(NextSize(size, false), minSize);
    }

    // Create four new Quadrants inside current Quadrant
    private void Split(QuadrantSize size, QuadrantSize minSize)
    {
        bottomLeft = new Quadrant(size, minSize, Vector2Int.zero, this, "0");
        bottomRight = new Quadrant(size, minSize, Vector2Int.right, this, "1");
        topLeft = new Quadrant(size, minSize, Vector2Int.up, this, "2");
        topRight = new Quadrant(size, minSize, Vector2Int.one, this, "3");
    }

    public List<Quadrant> GetQuadrants(bool recursive)
    {
        List<Quadrant> quadrants = new List<Quadrant>();

        if (recursive)
        {
            quadrants.Add(this);
            if (bottomLeft != null)
                quadrants.AddRange(bottomLeft.GetQuadrants(recursive));
            if (bottomRight != null)
                quadrants.AddRange(bottomRight.GetQuadrants(recursive));
            if (topLeft != null)
                quadrants.AddRange(topLeft.GetQuadrants(recursive));
            if (topRight != null)
                quadrants.AddRange(topRight.GetQuadrants(recursive));
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
        if (bottomLeft != null)
            leaves.AddRange(bottomLeft.GetLeafQuadrants());
        if (bottomRight != null)
            leaves.AddRange(bottomRight.GetLeafQuadrants());
        if (topLeft != null)
            leaves.AddRange(topLeft.GetLeafQuadrants());
        if (topRight != null)
            leaves.AddRange(topRight.GetLeafQuadrants());

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
            if (bottomLeft != null)
                quadrantsOfSize.AddRange(bottomLeft.FindQuadrantsOfSize(size));
            if (bottomRight != null)
                quadrantsOfSize.AddRange(bottomRight.FindQuadrantsOfSize(size));
            if (topLeft != null)
                quadrantsOfSize.AddRange(topLeft.FindQuadrantsOfSize(size));
            if (topRight != null)
                quadrantsOfSize.AddRange(topRight.FindQuadrantsOfSize(size));
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

        if (bottomLeft != null)
            bottomLeft.GenerateTerrainData();
        if (bottomRight != null)
            bottomRight.GenerateTerrainData();
        if (topLeft != null)
            topLeft.GenerateTerrainData();
        if (topRight != null)
            topRight.GenerateTerrainData();
    }

    public void GenerateTerrainTexture()
    {
        if (colorMap != null && colorMap.Length > 0)
        {
            texture = new Texture2D(textureSize, textureSize)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(colorMap);
            texture.Apply();
        }

        if (bottomLeft != null)
            bottomLeft.GenerateTerrainTexture();
        if (bottomRight != null)
            bottomRight.GenerateTerrainTexture();
        if (topLeft != null)
            topLeft.GenerateTerrainTexture();
        if (topRight != null)
            topRight.GenerateTerrainTexture();
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

    public void BuildColorMaps()
    {
        if (bottomLeft.colorMap == null)
            bottomLeft.BuildColorMaps();
        if (bottomRight.colorMap == null)
            bottomRight.BuildColorMaps();
        if (topLeft.colorMap == null)
            topLeft.BuildColorMaps();
        if (topRight.colorMap == null)
            topRight.BuildColorMaps();

        // Create colorMap of all children
        Color[] combinedColorMap = new Color[textureSize * textureSize * 4];
        for (int i = 0; i < bottomLeft.colorMap.Length; i++)
            combinedColorMap[((Mathf.FloorToInt(i / textureSize)) * textureSize * 2) + (i % textureSize)] = bottomLeft.colorMap[i];
        for (int i = 0; i < bottomRight.colorMap.Length; i++)
            combinedColorMap[((Mathf.FloorToInt(i / textureSize)) * textureSize * 2) + textureSize + (i % textureSize)] = bottomRight.colorMap[i];
        for (int i = 0; i < topLeft.colorMap.Length; i++)
            combinedColorMap[((textureSize + Mathf.FloorToInt(i / textureSize)) * textureSize * 2) + (i % textureSize)] = topLeft.colorMap[i];
        for (int i = 0; i < topRight.colorMap.Length; i++)
            combinedColorMap[((textureSize + Mathf.FloorToInt(i / textureSize)) * textureSize * 2) + textureSize + (i % textureSize)] = topRight.colorMap[i];

        // Shrink colorMap by combining pixels
        colorMap = new Color[textureSize * textureSize];
        for (int i = 0; i < colorMap.Length; i++)
        {
            int rowIncrement = (Mathf.FloorToInt(i / textureSize)) * textureSize * 4;
            int columnIncrement = (i % textureSize) * 2;
            float red = (combinedColorMap[columnIncrement + rowIncrement].r + combinedColorMap[columnIncrement + 1 + rowIncrement].r + combinedColorMap[columnIncrement + (textureSize * 2) + rowIncrement].r + combinedColorMap[columnIncrement + 1 + (textureSize * 2) + rowIncrement].r) / 4;
            float green = (combinedColorMap[columnIncrement + rowIncrement].g + combinedColorMap[columnIncrement + 1 + rowIncrement].g + combinedColorMap[columnIncrement + (textureSize * 2) + rowIncrement].g + combinedColorMap[columnIncrement + 1 + (textureSize * 2) + rowIncrement].g) / 4;
            Color color = new Color(red, green, 0f);
            colorMap[i] = color;
        }
    }

    public void SaveTerrainToPNG(string filePathBase)
    {
        Debug.Log("save file");
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + filePathBase + "/";
        string fileName = id == "" ? "Quadrant" : "Quadrant_" + id;

        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);

        File.WriteAllBytes(dirPath + fileName + ".png", bytes);

        if (bottomLeft != null)
            bottomLeft.SaveTerrainToPNG(filePathBase);
        if (bottomRight != null)
            bottomRight.SaveTerrainToPNG(filePathBase);
        if (topLeft != null)
            topLeft.SaveTerrainToPNG(filePathBase);
        if (topRight != null)
            topRight.SaveTerrainToPNG(filePathBase);
    }

    public void LoadTerrainFromPNG(string filePathBase)
    {
        Texture2D texture = null;
        byte[] fileData;
        string fileName = id == "" ? "Quadrant" : "Quadrant_" + id;
        string filePath = Application.dataPath + filePathBase + fileName + ".png";

        if (!File.Exists(filePath))
        {
            Debug.LogError(filePath);
            Debug.LogError("filePath does not exist");
            return;
        }

        fileData = File.ReadAllBytes(filePath);
        texture = new Texture2D(2, 2); // Dimensions don't matter, LoadImage auto-resizes
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.LoadImage(fileData);

        this.texture = texture;
        colorMap = texture.GetPixels();

        if (bottomLeft != null)
            bottomLeft.LoadTerrainFromPNG(filePathBase);
        if (bottomRight != null)
            bottomRight.LoadTerrainFromPNG(filePathBase);
        if (topLeft != null)
            topLeft.LoadTerrainFromPNG(filePathBase);
        if (topRight != null)
            topRight.LoadTerrainFromPNG(filePathBase);
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

    public void TryPaint(Vector2Int position, TerrainBrushMode mode)
    {
        Color pixelColor = texture.GetPixel(position.x, position.y);
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
            TerrainQuadTree.Instance.PaintSend(position, pixelColorNew, id);
        }
    }

    public void EditTerrain(Vector2Int position, Color color)
    {
        if (terrainChanges.ContainsKey(position))
            terrainChanges[position] = color;
        else
            terrainChanges.Add(position, color);
    }

    public void UpdateTerrainChanges()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        List<Color> colors = new List<Color>();

        // Check for every terrain change, for every unit, if the unit is in range to see the change
        foreach (var pair in terrainChanges)
            foreach (Unit unit in Game.Instance.Units) //TODO: change it so that if a unit has been found that sees one change, put him at the front of the list, because changes are often close to eachother
                if (Vector2.Distance(new Vector2(unit.transform.position.x, unit.transform.position.z), new Vector2(position.x + pair.Key.x, position.y + pair.Key.y)) < unit.range)
                {
                    positions.Add(pair.Key);
                    colors.Add(pair.Value);
                }

        if (positions.Count > 0)
            ApplyTerrainChange(positions.ToArray(), colors.ToArray());
    }

    public void ApplyAllTerrainChanges()
    {
        Vector2Int[] positions = new Vector2Int[terrainChanges.Count];
        Color[] colors = new Color[terrainChanges.Count];

        int i = 0;
        foreach (var pair in terrainChanges)
        {
            positions[i] = pair.Key;
            colors[i] = pair.Value;
            i++;
        }

        ApplyTerrainChange(positions, colors);
    }

    public void ApplyTerrainChange(Vector2Int[] positions, Color[] colors)
    {
        Debug.Log("apply");
        for (int i = 0; i < positions.Length; i++)
        {
            texture.SetPixel(positions[i].x, positions[i].y, colors[i]);
            terrainChanges.Remove(positions[i]);
        }
        texture.Apply();
        TerrainQuadTree.Instance.UpdateTerrainTexture(this);

        // Find neighbors of positions and combine the colors
        List<Vector2Int> parentPositions = new List<Vector2Int>();
        List<Color> combinedColors = new List<Color>();
        for (int i = 0; i < positions.Length; i++)
        {
            string lastChar = id.Substring(id.Length - 1);
            Vector2Int basePos = new Vector2Int(Mathf.FloorToInt(positions[i].x / 2) * 2, Mathf.FloorToInt(positions[i].y / 2) * 2);
            Color combinedColor = (texture.GetPixel(basePos.x, basePos.y) + texture.GetPixel(basePos.x + 1, basePos.y) + texture.GetPixel(basePos.x, basePos.y + 1) + texture.GetPixel(basePos.x + 1, basePos.y + 1)) / 4;
            Vector2Int basePosOffset = new Vector2Int(lastChar == "1" || lastChar == "3" ? textureSize : 0, lastChar == "2" || lastChar == "3" ? textureSize : 0);
            basePos.x += basePosOffset.x;
            basePos.y += basePosOffset.y;
            Vector2Int parentPosition = new Vector2Int(basePos.x / 2, basePos.y / 2);
            if (!parentPositions.Contains(parentPosition))
            {
                parentPositions.Add(parentPosition);
                combinedColors.Add(combinedColor);
            }
        }
        parent.ApplyTerrainChangeRecursive(parentPositions, combinedColors);
    }

    private void ApplyTerrainChangeRecursive(List<Vector2Int> positions, List<Color> colors)
    {
        List<Vector2Int> remainingPositions = new List<Vector2Int>();

        for (int i = 0; i < positions.Count; i++)
        {
            bool edited = false;
            if (texture.GetPixel(positions[i].x, positions[i].y) != colors[i]) //TODO: Make sure similar colors actually return false here (rounding issues)
            {
                texture.SetPixel(positions[i].x, positions[i].y, colors[i]);
                edited = true;
            }

            if (edited)
                remainingPositions.Add(positions[i]);
        }

        // Apply changes and call recursively if any edits were made
        if (remainingPositions.Count > 0)
        {
            texture.Apply();

            if (parent != null)
            {
                TerrainQuadTree.Instance.UpdateTerrainTexture(this);

                List<Vector2Int> parentPositions = new List<Vector2Int>();
                List<Color> combinedColors = new List<Color>();
                for (int i = 0; i < remainingPositions.Count; i++)
                {
                    string lastChar = id.Substring(id.Length - 1);
                    Vector2Int basePos = new Vector2Int(Mathf.FloorToInt(remainingPositions[i].x / 2) * 2, Mathf.FloorToInt(remainingPositions[i].y / 2) * 2);
                    Color combinedColor = (texture.GetPixel(basePos.x, basePos.y) + texture.GetPixel(basePos.x + 1, basePos.y) + texture.GetPixel(basePos.x, basePos.y + 1) + texture.GetPixel(basePos.x + 1, basePos.y + 1)) / 4;
                    Vector2Int basePosOffset = new Vector2Int(lastChar == "1" || lastChar == "3" ? textureSize : 0, lastChar == "2" || lastChar == "3" ? textureSize : 0);
                    basePos.x += basePosOffset.x;
                    basePos.y += basePosOffset.y;
                    Vector2Int parentPosition = new Vector2Int(basePos.x / 2, basePos.y / 2);
                    if (!parentPositions.Contains(parentPosition))
                    {
                        parentPositions.Add(parentPosition);
                        combinedColors.Add(combinedColor);
                    }
                }
                parent.ApplyTerrainChangeRecursive(parentPositions, combinedColors); //TODO: this call doesn't work. The 2nd time it goes through this recursive call, it breaks and doesn't display the changes.
            }
        }
    }

    private Vector2Int[] GetGlobalPositions(Vector2Int[] positions)
    {
        Vector2Int[] globalPositions = new Vector2Int[positions.Length];
        for (int i = 0; i < globalPositions.Length; i++)
            globalPositions[i] = new Vector2Int(position.x + this.position.x, position.y + this.position.y);
        return globalPositions;
    }
}
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadrant
{
    public Vector2Int position;
    public Vector2Int size;
    private Quadrant parent;
    private int depth;

    private Quadrant topLeft;
    private Quadrant topRight;
    private Quadrant bottomLeft;
    private Quadrant bottomRight;

    public Texture2D texture; // Map data texture

    private float[,] noiseMap;

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int offset, Vector2Int position)
    {
        this.position = position;
        this.size = new Vector2Int((int)size, (int)size);

        if ((int)size > (int)minSize)
            Split(NextSize(size), minSize);
    }

    public Quadrant(QuadrantSize size, QuadrantSize minSize, Vector2Int offset, Quadrant parent)
    {
        this.parent = parent;
        position = new Vector2Int(this.parent.position.x + (int)size * offset.x, this.parent.position.y + (int)size * offset.y);
        this.size = new Vector2Int((int)size, (int)size);

        if ((int)size > (int)minSize)
            Split(NextSize(size), minSize);
    }

    // Create four new Quadrants inside current Quadrant
    private void Split(QuadrantSize size, QuadrantSize minSize)
    {
        topLeft = new Quadrant(size, minSize, Vector2Int.up, this);
        topRight = new Quadrant(size, minSize, Vector2Int.one, this);
        bottomLeft = new Quadrant(size, minSize, Vector2Int.zero, this);
        bottomRight = new Quadrant(size, minSize, Vector2Int.right, this);
    }

    public List<Quadrant> GetQuadrants()
    {
        List<Quadrant> quadrants = new List<Quadrant>();

        quadrants.Add(this);
        if (topLeft != null)
            quadrants.AddRange(topLeft.GetQuadrants());
        if (topRight != null)
            quadrants.AddRange(topRight.GetQuadrants());
        if (bottomLeft != null)
            quadrants.AddRange(bottomLeft.GetQuadrants());
        if (bottomRight != null)
            quadrants.AddRange(bottomRight.GetQuadrants());

        return quadrants;
    }

    public void GenerateNoiseMap()
    {
        noiseMap = Noise.GenerateNoiseMap(new Vector2Int((int)QuadrantSize.k4, (int)QuadrantSize.k4), 0, 0, 1, 0.5f, 1, Vector2.zero);

        if (topLeft != null)
            topLeft.GenerateNoiseMap();
        if (topRight != null)
            topRight.GenerateNoiseMap();
        if (bottomLeft != null)
            bottomLeft.GenerateNoiseMap();
        if (bottomRight != null)
            bottomRight.GenerateNoiseMap();
    }

    // Grab next smallest size from QuadrantSize enum
    private QuadrantSize NextSize(QuadrantSize size)
    {
        QuadrantSize[] list = (QuadrantSize[])Enum.GetValues(typeof(QuadrantSize));
        return list[Array.IndexOf<QuadrantSize>(list, size) - 1];
    }
}
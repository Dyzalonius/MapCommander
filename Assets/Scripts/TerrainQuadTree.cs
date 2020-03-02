using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainQuadTree : MonoBehaviour
{
    private Rect bounds;
    private Quadrant root;
    private int maxDepth;

    public TerrainQuadTree()
    {

    }

    internal class Quadrant
    {
        private Rect bounds;
        private Quadrant parent;
        private int depth;

        private Quadrant topLeft;
        private Quadrant topRight;
        private Quadrant bottomLeft;
        private Quadrant bottomRight;

        private Texture2D texture; // Store mapData here in a texture of 1000x1000

        public Quadrant()
        {
            //if (true)
            //    Split();
        }

        private void Split()
        {
            topLeft = new Quadrant();
            topRight = new Quadrant();
            bottomLeft = new Quadrant();
            bottomRight = new Quadrant();
        }
    }

    public void BuildTree()
    {
        //root = new
    }
}

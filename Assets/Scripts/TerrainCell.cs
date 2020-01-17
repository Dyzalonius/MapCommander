using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCell
{
    public Vector2Int Coordinates;
    public bool isForest;

    private Mesh forestMesh;

    public TerrainCell(Vector2Int coordinates)
    {
        Coordinates = coordinates;
        isForest = Random.value > 0.8f; // Temporary method to make a cell a forest
    }

    public void DrawForest()
    {
        // Exit if not a forest
        if (!isForest)
            return;

        if (forestMesh == null)
        {
            forestMesh = new Mesh();
            Vector3 pos = new Vector3(Coordinates.x * TerrainGrid.Instance.CellSize, 0, Coordinates.y * TerrainGrid.Instance.CellSize);
            float size = TerrainGrid.Instance.CellSize;
            forestMesh.vertices = new Vector3[] { new Vector3(pos.x, 0.1f, pos.z), new Vector3(pos.x + size, 0.1f, pos.z), new Vector3(pos.x, 0.1f, pos.z + size), new Vector3(pos.x + size, 0.1f, pos.z + size) };
            forestMesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            forestMesh.triangles = new int[6] { 0, 2, 1, 2, 3, 1 };
        }

        Graphics.DrawMesh(forestMesh, Vector3.zero, Quaternion.identity, TerrainGrid.Instance.ForestMaterial, 9);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBrush : MonoBehaviour
{
    [SerializeField]
    private int brushSize;

    [HideInInspector]
    public TerrainBrushMode Mode = TerrainBrushMode.NONE;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
            ToggleMode();

        if (Input.GetMouseButton(0) && Mode != TerrainBrushMode.NONE)
            Paint();
    }

    private void ToggleMode()
    {
        TerrainBrushMode[] list = (TerrainBrushMode[])Enum.GetValues(typeof(TerrainBrushMode));
        int index = Array.IndexOf<TerrainBrushMode>(list, Mode) + 1;
        if (index == list.Length)
            index = 0;

        Mode = list[index];
    }

    private void Paint()
    {
        foreach (Vector2Int position in FindPositions())
            TerrainQuadTree.Instance.TryPaint(position, Mode);
    }

    private List<Vector2Int> FindPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int centerPos = new Vector2Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.z));
        for (int i = -brushSize + 1; i < brushSize; i++)
            for (int j = -brushSize + 1; j < brushSize; j++)
                positions.Add(new Vector2Int(centerPos.x + i, centerPos.y + j));

        return positions;
    }

    private void OnValidate()
    {
        if (brushSize < 1)
            brushSize = 1;

        if (brushSize > 10)
            brushSize = 10;
    }
}

public enum TerrainBrushMode
{
    NONE,
    ADDFOREST,
    REMOVEFOREST
}
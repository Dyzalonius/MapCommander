using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBrush : MonoBehaviour
{
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
        positions.Add(new Vector2Int(Mathf.FloorToInt(mousePos.x), Mathf.FloorToInt(mousePos.z)));

        return positions;
    }
}

public enum TerrainBrushMode
{
    NONE,
    ADDFOREST,
    REMOVEFOREST
}
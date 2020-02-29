using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MapGrid : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Material lineMaterial;

    [SerializeField]
    private Camera cam;

    [Header("Settings")]
    [SerializeField]
    private int gridSize;

    [SerializeField]
    private int gridCellSize;

    [SerializeField]
    private Vector2 gridPadding;

    [SerializeField]
    private Color[] gridLevelColors;

    [SerializeField]
    private float[] gridLevelCameraScales;

    [HideInInspector]
    public List<Vector3> MapGridMarkerPositions = new List<Vector3>();

    [HideInInspector]
    public int SmallestGridLevel;

    private int smallestGridCellSize;
    private Vector2 camMinima;
    private Vector2 camMaxima;
    private Vector2 camMinimaWithPadding;
    private Vector2 camMaximaWithPadding;

    public void DrawGrid()
    {
        FindCamMinimaMaxima();
        FindSmallestGridLevel();
        FindSmallestGridCellSize();
        MapGridMarkerPositions.Clear();

        // When adding to MapGridMarkerPositions, it always stores the 0,min; 0,max; min,0; max,0; startmin, 0;

        for (int i = GetGridValue(camMinima.x); i < GetGridValue(camMaxima.x); i += smallestGridCellSize)
        {
            Color color = i % (gridCellSize * Mathf.Pow(10, SmallestGridLevel + 1)) == 0 ? gridLevelColors[1] : gridLevelColors[0];
            Vector3 startPos = new Vector3(i, 0, camMinimaWithPadding.y);
            Vector3 endPos = new Vector3(i, 0, camMaximaWithPadding.y);
            DrawLine(startPos, endPos, color);
            startPos.z = int.MinValue;
            endPos.z = int.MaxValue;
            MapGridMarkerPositions.Add(startPos);
            MapGridMarkerPositions.Add(endPos);
        }

        for (int i = GetGridValue(camMinima.y); i < GetGridValue(camMaxima.y); i += smallestGridCellSize)
        {
            Color color = i % (gridCellSize * Mathf.Pow(10, SmallestGridLevel + 1)) == 0 ? gridLevelColors[1] : gridLevelColors[0];
            Vector3 startPos = new Vector3(camMinimaWithPadding.x, 0, i);
            Vector3 endPos = new Vector3(camMaximaWithPadding.x, 0, i);
            DrawLine(startPos, endPos, color);
            startPos.x = int.MinValue;
            endPos.x = int.MaxValue;
            MapGridMarkerPositions.Add(startPos);
            MapGridMarkerPositions.Add(endPos);
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
    {
        GL.Begin(GL.LINES);
        lineMaterial.SetPass(0);
        GL.Color(color);
        GL.Vertex3(startPos.x, startPos.y, startPos.z);
        GL.Vertex3(endPos.x, endPos.y, endPos.z);
        GL.End();
    }

    private void FindCamMinimaMaxima()
    {
        Vector3 camMinimum = cam.ScreenToWorldPoint(new Vector3(0, 0));
        Vector3 camMaximum = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight));
        camMinima.x = camMinimum.x;
        camMinima.y = camMinimum.z;
        camMaxima.x = camMaximum.x;
        camMaxima.y = camMaximum.z;

        Vector3 camMinimumWithPadding = cam.ScreenToWorldPoint(new Vector3(gridPadding.x * (3 - SmallestGridLevel), gridPadding.y));
        Vector3 camMaximumWithPadding = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth - gridPadding.x * (3 - SmallestGridLevel), cam.pixelHeight - gridPadding.y));
        camMinimaWithPadding.x = camMinimumWithPadding.x;
        camMinimaWithPadding.y = camMinimumWithPadding.z;
        camMaximaWithPadding.x = camMaximumWithPadding.x;
        camMaximaWithPadding.y = camMaximumWithPadding.z;
    }

    private void FindSmallestGridCellSize()
    {
        smallestGridCellSize = Mathf.FloorToInt(gridCellSize * Mathf.Pow(10, SmallestGridLevel));
    }

    private void FindSmallestGridLevel()
    {
        int gridLevel = 0;
        foreach (float cameraScale in gridLevelCameraScales)
            if (cam.orthographicSize > cameraScale)
                gridLevel++;

        SmallestGridLevel = gridLevel;
    }

    private int GetGridValue(float value)
    {
        float difference = value % smallestGridCellSize;
        return value > 0 ? Mathf.RoundToInt(value - difference + smallestGridCellSize) : Mathf.RoundToInt(value - difference);
    }
}

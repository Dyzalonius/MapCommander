using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    [SerializeField]
    private Color innerColor; // Selection box inner color

    [SerializeField]
    private Color borderColor; // Selection box border color

    [SerializeField]
    private float borderThickness; // Selection box border color

    [SerializeField]
    private Camera cam;

    [HideInInspector]
    public List<Unit> Units = new List<Unit>();

    private bool isSelecting = false;
    private Vector3 mousePosStart;
    private Texture2D whiteTexture; // Texture used for drawing rectangles
    
    private void Start()
    {
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartSelection();

        if (Input.GetMouseButtonUp(0))
            FinishSelection();
    }

    private void OnGUI()
    {

        if (isSelecting)
            DrawSelectionBox(mousePosStart, Input.mousePosition);
    }

    private void StartSelection()
    {
        // Start selection box
        mousePosStart = Input.mousePosition;
        isSelecting = true;
    }

    private void FinishSelection()
    {
        List<Unit> units = Game.Instance.Units;
        List<Unit> foundUnits = new List<Unit>();

        // Find units
        units.ForEach(x =>
        {
            if (IsInSelection(x.gameObject))
                foundUnits.Add(x);
        });

        // Select found units
        SelectUnits(foundUnits);

        // End selection box
        isSelecting = false;
    }

    private void DeselectUnits()
    {
        Units.ForEach(x => x.Deselect());
        Units.Clear();
    }

    private void SelectUnits(List<Unit> units)
    {
        DeselectUnits();
        units.ForEach(x => { Units.Add(x); x.Select(); });
    }

    private bool IsInSelection(GameObject gameObject)
    {
        Bounds viewportBounds = GetViewportBounds(mousePosStart, Input.mousePosition);

        Vector3 position = cam.WorldToViewportPoint(gameObject.transform.position);
        bool contains = viewportBounds.Contains(position);
        return contains;
    }

    // Returns viewport bounds from one given position to another
    private Bounds GetViewportBounds(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        Vector3 v1 = cam.ScreenToViewportPoint(screenPosition1);
        Vector3 v2 = cam.ScreenToViewportPoint(screenPosition2);
        Vector3 min = Vector3.Min(v1, v2);
        Vector3 max = Vector3.Max(v1, v2);
        min.z = cam.nearClipPlane;
        max.z = cam.farClipPlane;

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    private void DrawSelectionBox(Vector3 pos1, Vector3 pos2)
    {
        // Move origin from bottom left to top left
        pos1.y = Screen.height - pos1.y;
        pos2.y = Screen.height - pos2.y;

        // Calculate corners
        Vector3 topLeft = Vector3.Min(pos1, pos2);
        Vector3 bottomRight = Vector3.Max(pos1, pos2);

        // Create rect
        Rect rect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);

        // Draw
        GUI.color = innerColor;
        GUI.DrawTexture(rect, whiteTexture); // Center
        GUI.color = borderColor;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, borderThickness), whiteTexture); // Top
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, borderThickness, rect.height), whiteTexture); // Left
        GUI.DrawTexture(new Rect(rect.xMax - borderThickness, rect.yMin, borderThickness, rect.height), whiteTexture); // Right
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - borderThickness, rect.width, borderThickness), whiteTexture); // Bottom
        GUI.color = Color.white;
    }
}

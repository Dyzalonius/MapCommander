using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRangeCircles : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Material lineMaterial;

    [Header("Settings")]
    [SerializeField]
    private int vertexCount;

    public void DrawCircles()
    {
        foreach (Unit unit in Game.Instance.Player.Selection.Units)
            DrawCircle(unit.transform.position, unit.range);
    }

    private void DrawCircle(Vector3 position, float range)
    {
        Vector3 startPos = new Vector3(position.x + (Mathf.Cos(0) * range), 0f, position.z + (Mathf.Sin(0) * range));
        for (int i = 0; i < vertexCount; i++)
        {
            float factor = (float)(i + 1) / (float)vertexCount;
            Vector3 endPos = new Vector3(position.x + (Mathf.Cos(factor * Mathf.PI * 2) * range), 0f, position.z + (Mathf.Sin(factor * Mathf.PI * 2) * range));
            DrawLine(startPos, endPos, Color.black);
            startPos = endPos;
        }
    }

    private void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
    {
        GL.Begin(GL.LINES);
        lineMaterial.SetPass(0);
        GL.Color(color);
        GL.Vertex3(startPos.x, 0, startPos.z);
        GL.Vertex3(endPos.x, 0, endPos.z);
        GL.End();
    }

    private void OnValidate()
    {
        if (vertexCount < 3)
            vertexCount = 3;
    }
}

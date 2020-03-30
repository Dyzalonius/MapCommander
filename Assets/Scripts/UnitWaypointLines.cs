using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWaypointLines : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Material lineMaterial;

    public void DrawLines()
    {
        if (Game.Instance == null)
            return;

        foreach (Unit unit in Game.Instance.Units)
        {
            Vector3 startPos = unit.transform.position;
            foreach (UnitWaypoint waypoint in unit.Waypoints)
            {
                DrawLine(startPos, waypoint.Position, Color.black);
                startPos = waypoint.Position;
            }
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
}

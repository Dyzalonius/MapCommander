using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    public Camera Cam;

    [HideInInspector]
    public Selection Selection;

    private Vector3 mousePos;

    private void Update()
    {
        mousePos = Cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonUp(1))
            GiveUnitOrders(Input.GetKey(KeyCode.LeftShift));
    }

    private void GiveUnitOrders(bool modifier)
    {
        if (Selection.Units.Count == 0 || mousePos == Vector3.zero)
            return;

        // Give waypoint
        Selection.Units.ForEach(x => {
            // Remove existing waypoints if modifier is not pressed
            if (!modifier)
                x.Waypoints.Clear();

            x.Waypoints.Enqueue(mousePos);
        });
    }
}

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
        UpdateMousePos();

        if (Input.GetMouseButtonUp(1))
            GiveUnitOrders();
    }

    private void UpdateMousePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
            mousePos = hit.point;
        else
            mousePos = Vector3.zero;
    }

    private void GiveUnitOrders()
    {
        if (Selection.Units.Count == 0 || mousePos == Vector3.zero)
            return;
        
        Selection.Units.ForEach(x => x.MoveTarget = mousePos);
    }
}

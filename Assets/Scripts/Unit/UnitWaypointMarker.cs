using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWaypointMarker : MonoBehaviour
{
    private UnitWaypoint unitWaypoint;

    private void Update()
    {
        UpdateMarker();
    }

    public void Setup(UnitWaypoint unitWaypoint)
    {
        this.unitWaypoint = unitWaypoint;
        UpdateMarker();
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void UpdateMarker()
    {
        transform.position = Camera.main.WorldToScreenPoint(unitWaypoint.Position);
    }
}

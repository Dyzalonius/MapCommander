using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWaypointMarkers : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject unitWaypointMarkerPrefab = null;

    private List<UnitWaypointMarker> unitWaypointMarkerPool = new List<UnitWaypointMarker>();
    private List<UnitWaypoint> unitWaypoints = new List<UnitWaypoint>();
    private Dictionary<UnitWaypoint, UnitWaypointMarker> unitWaypointMarkers = new Dictionary<UnitWaypoint, UnitWaypointMarker>();

    private void Update()
    {
        // Get list of all waypoints
        List<UnitWaypoint> unitWaypoints = new List<UnitWaypoint>();
        foreach (Unit unit in Game.Instance.Units)
            foreach (UnitWaypoint unitWaypoint in unit.Waypoints)
                unitWaypoints.Add(unitWaypoint);

        // Remove markers for old waypoints
        for (int i = this.unitWaypoints.Count - 1; i >= 0; i--)
            if (!unitWaypoints.Contains(this.unitWaypoints[i]))
                RemoveUnitWaypointMarker(this.unitWaypoints[i]);

        // Add markers for new waypoints
        foreach (UnitWaypoint unitWaypoint in unitWaypoints)
            if (!unitWaypointMarkers.ContainsKey(unitWaypoint))
                AddUnitWaypointMarker(unitWaypoint);
    }

    // Add unitmarker for given unit
    private void AddUnitWaypointMarker(UnitWaypoint unitWaypoint)
    {
        if (unitWaypointMarkerPool.Count == 0)
            MakeNewUnitWaypointMarker();

        UnitWaypointMarker unitWaypointMarker = unitWaypointMarkerPool[0];
        unitWaypointMarkerPool.Remove(unitWaypointMarker);
        unitWaypointMarker.Setup(unitWaypoint);
        unitWaypoints.Add(unitWaypoint);
        unitWaypointMarkers.Add(unitWaypoint, unitWaypointMarker);
    }

    // Remove unitmarker of given unit and put it back in the pool
    private void RemoveUnitWaypointMarker(UnitWaypoint unitWaypoint)
    {
        UnitWaypointMarker unitMarker = unitWaypointMarkers[unitWaypoint];
        unitMarker.Deactivate();
        unitWaypointMarkers.Remove(unitWaypoint);
        unitWaypoints.Remove(unitWaypoint);
        unitWaypointMarkerPool.Add(unitMarker);
    }

    // Instantiate new unitmarker and add it to the pool
    private void MakeNewUnitWaypointMarker()
    {
        UnitWaypointMarker newUnitWaypointMarker = Instantiate(unitWaypointMarkerPrefab, transform).GetComponent<UnitWaypointMarker>();
        unitWaypointMarkerPool.Add(newUnitWaypointMarker);
    }
}

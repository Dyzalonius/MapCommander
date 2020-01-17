using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMarkers : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject unitMarkerPrefab;

    private List<UnitMarker> unitMarkerPool = new List<UnitMarker>();
    private Dictionary<Unit, UnitMarker> unitMarkers = new Dictionary<Unit, UnitMarker>();

    private void Update()
    {
        foreach (Unit unit in Game.Instance.Units)
        {
            if (!unitMarkers.ContainsKey(unit))
                AddUnitMarker(unit);
        }
    }

    // Add unitmarker for given unit
    private void AddUnitMarker(Unit unit)
    {
        if (unitMarkerPool.Count == 0)
            MakeNewUnitMarker();

        UnitMarker unitMarker = unitMarkerPool[0];
        unitMarkerPool.Remove(unitMarker);
        unitMarker.Setup(unit);
        unitMarkers.Add(unit, unitMarker);
    }

    // Remove unitmarker of given unit and put it back in the pool
    private void RemoveUnitMarker(Unit unit)
    {
        UnitMarker unitMarker = unitMarkers[unit];
        unitMarkers.Remove(unit);
        unitMarkerPool.Add(unitMarker);
    }

    // Instantiate new unitmarker and add it to the pool
    private void MakeNewUnitMarker()
    {
        UnitMarker newUnitMarker = Instantiate(unitMarkerPrefab, transform).GetComponent<UnitMarker>();
        unitMarkerPool.Add(newUnitMarker);
    }
}

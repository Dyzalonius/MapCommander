using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject mapGridMarkerPrefab;

    [SerializeField]
    private MapGrid mapGrid;

    [SerializeField]
    private Camera cam;

    private List<MapGridMarker> mapGridMarkerPool = new List<MapGridMarker>();
    private List<Vector3> mapGridPositions = new List<Vector3>();
    private Dictionary<Vector3, MapGridMarker> mapGridMarkers = new Dictionary<Vector3, MapGridMarker>();

    private void Update()
    {
        // Recycle old markers
        for (int i = mapGridPositions.Count - 1; i >= 0; i--)
        {
            if (!mapGrid.MapGridMarkerPositions.Contains(mapGridPositions[i]))
                RemoveMapGridMarker(mapGridPositions[i]);
        }

        // Add new markers
        foreach (Vector3 position in mapGrid.MapGridMarkerPositions)
        {
            if (!mapGridMarkers.ContainsKey(position))
                AddMapGridMarker(position);
        }
    }

    private void AddMapGridMarker(Vector3 position)
    {
        if (mapGridMarkerPool.Count == 0)
            MakeNewMapGridMarker();

        MapGridMarker mapGridMarker = mapGridMarkerPool[0];
        mapGridMarkerPool.Remove(mapGridMarker);
        mapGridMarker.enabled = true;
        mapGridMarker.Setup(position, mapGrid, cam);
        mapGridPositions.Add(position);
        mapGridMarkers.Add(position, mapGridMarker);
    }

    private void MakeNewMapGridMarker()
    {
        MapGridMarker newMapGridMarker = Instantiate(mapGridMarkerPrefab, transform).GetComponent<MapGridMarker>();
        mapGridMarkerPool.Add(newMapGridMarker);
    }

    private void RemoveMapGridMarker(Vector3 position)
    {
        MapGridMarker mapGridMarker = mapGridMarkers[position];
        mapGridMarker.Deactivate();
        mapGridPositions.Remove(position);
        mapGridMarkers.Remove(position);
        mapGridMarkerPool.Add(mapGridMarker);
    }
}

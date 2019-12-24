using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGridMarker : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Vector2 positionOffsets;

    private Vector3 position;
    private MapGrid mapGrid;
    private Camera cam;
    private Vector3 positionOffset;
    private string coordinate;

    private void Update()
    {
        UpdateMarker();
    }

    public void Setup(Vector3 position, MapGrid mapGrid, Camera cam)
    {
        text.enabled = true;
        this.position = position;
        this.mapGrid = mapGrid;
        this.cam = cam;
        coordinate = GetCoordinate(position);
        SetTextAnchor();
    }

    public void Deactivate()
    {
        text.enabled = false;
        enabled = false;
    }

    private void UpdateMarker()
    {
        positionOffset = Vector3.zero;

        if (position.x == int.MinValue)
            positionOffset.x = positionOffsets.x;
        if (position.x == int.MaxValue)
            positionOffset.x = -positionOffsets.x;
        if (position.z == int.MinValue)
            positionOffset.y = positionOffsets.y;
        if (position.z == int.MaxValue)
            positionOffset.y = -positionOffsets.y;

        transform.position = cam.WorldToScreenPoint(FindPosInCamera(position)) + positionOffset;
        text.text = GetMarkerText();
    }

    private void SetTextAnchor()
    {
        if (position.x == int.MinValue)
            text.alignment = TextAnchor.MiddleLeft;
        if (position.x == int.MaxValue)
            text.alignment = TextAnchor.MiddleRight;
        if (position.z == int.MinValue)
            text.alignment = TextAnchor.LowerCenter;
        if (position.z == int.MaxValue)
            text.alignment = TextAnchor.UpperCenter;
    }

    // coordinate system is from 00000m to 99999m, and the smallest scale is 100, so it goes from 000 till 999
    private string GetCoordinate(Vector3 position)
    {
        float coordinateValue = 0;

        if (position.x == int.MinValue || position.x == int.MaxValue)
            coordinateValue = position.z;
        if (position.z == int.MinValue || position.z == int.MaxValue)
            coordinateValue = position.x;
        while (coordinateValue < 0)
            coordinateValue = 100000 + coordinateValue;
        while (coordinateValue >= 100000)
            coordinateValue = coordinateValue - 100000;

        string newCoordinate = coordinateValue.ToString();

        for (int i = newCoordinate.Length; i < 5; i++)
            newCoordinate = "0" + newCoordinate;

        return newCoordinate;
    }

    private string GetMarkerText()
    {
        return coordinate.Substring(0, 3 - mapGrid.SmallestGridLevel);
    }

    private Vector3 FindPosInCamera(Vector3 position)
    {
        Vector3 camMinima = cam.ScreenToWorldPoint(Vector3.zero);
        Vector3 camMaxima = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight));
        position.x = Mathf.Clamp(position.x, camMinima.x, camMaxima.x);
        position.z = Mathf.Clamp(position.z, camMinima.z, camMaxima.z);
        return position;
    }

    private void OnDrawGizmos()
    {
        if (text.enabled)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cam.ScreenToWorldPoint(transform.position), 1f);
        }
    }
}

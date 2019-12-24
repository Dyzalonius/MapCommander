using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Image selectionMarker;

    private Unit unit;

    private void Update()
    {
        // Exit if unit is null
        if (!unit)
            return;

        UpdateMarker();
    }

    public void Setup(Unit unit)
    {
        this.unit = unit;
    }

    private void UpdateMarker()
    {
        transform.position = Camera.main.WorldToScreenPoint(unit.transform.position);
        selectionMarker.enabled = unit.IsSelected;
    }
}

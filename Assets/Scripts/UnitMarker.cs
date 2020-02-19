using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField]
    private Image onSelectedMarker;

    [SerializeField]
    private Image onHoverMarker;

    [SerializeField]
    private float hoverRotationStep = 2f;

    private Unit unit;

    private void Start()
    {
        onSelectedMarker.enabled = false;
        onHoverMarker.enabled = false;
    }

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
        onSelectedMarker.enabled = unit.IsSelected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(HoverRoutine());
        unit.IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverMarker.enabled = false;
        unit.IsHovered = false;
    }

    private IEnumerator HoverRoutine()
    {
        float rotation = 0f;
        onHoverMarker.enabled = true;
        while (onHoverMarker.enabled)
        {
            onHoverMarker.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            rotation -= hoverRotationStep;
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float camScaleMin;

    [SerializeField]
    private float camScaleMax;

    [SerializeField]
    private float camScaleDelta;

    [SerializeField]
    private float panSpeed;

    [SerializeField]
    private float dragFactor;

    private Camera cam
    {
        get
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            return _camera;
        }
    }
    private Camera _camera;
    private float cameraScale;
    private Vector3 dragStart, dragOrigin;
    private Vector3 defaultCameraPosition;

    private void Start()
    {
        cameraScale = cam.orthographicSize;
        defaultCameraPosition = transform.position;
    }

    private void Update()
    {
        if (Input.mouseScrollDelta.y > 0)
            ScrollForward();

        if (Input.mouseScrollDelta.y < 0)
            ScrollBackward();
        
        if (Input.GetMouseButtonDown(1))
        {
            dragStart = transform.position;
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
            DragMove();

        /*if (Input.GetKey(KeyCode.RightArrow) || cam.ScreenToViewportPoint(Input.mousePosition).x > 1)
            Pan(Vector3.right);

        if (Input.GetKey(KeyCode.LeftArrow) || cam.ScreenToViewportPoint(Input.mousePosition).x < 0)
            Pan(Vector3.left);

        if (Input.GetKey(KeyCode.UpArrow) || cam.ScreenToViewportPoint(Input.mousePosition).y > 1)
            Pan(Vector3.forward);

        if (Input.GetKey(KeyCode.DownArrow) || cam.ScreenToViewportPoint(Input.mousePosition).y < 0)
            Pan(Vector3.back);*/
    }

    private void ScrollBackward()
    {
        cameraScale += camScaleDelta * cameraScale;

        if (cameraScale > camScaleMax)
            cameraScale = camScaleMax;

        cam.orthographicSize = cameraScale;
    }

    private void ScrollForward()
    {
        cameraScale -= camScaleDelta * cameraScale;

        if (cameraScale < camScaleMin)
            cameraScale = camScaleMin;

        cam.orthographicSize = cameraScale;
    }

    private void DragMove()
    {
        if (Game.Instance.Player.Selection.Units.Count > 0)
            return;

        Vector3 offset = Input.mousePosition - dragOrigin;
        Vector3 newPos = defaultCameraPosition;
        newPos.x = dragStart.x - offset.x * (cameraScale / dragFactor);
        newPos.z = dragStart.z - offset.y * (cameraScale / dragFactor);

        transform.position = newPos;
    }

    private void Pan(Vector3 direction)
    {
        transform.Translate(direction * panSpeed * cameraScale, Space.World);
    }
}

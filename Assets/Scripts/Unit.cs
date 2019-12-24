using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private float movementSpeed; // in km/h

    [HideInInspector]
    public Vector3 MoveTarget;

    [HideInInspector]
    public bool IsSelected;

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 moveVector = (MoveTarget - transform.position);
        moveVector.y = 0;

        // Return if already at target
        if (moveVector.magnitude == 0)
            return;

        // Calculate speed in meters per fixed delta time
        float speed = movementSpeed / 3.6f * Time.fixedDeltaTime;

        // Move with speed up to movement speed
        if (moveVector.magnitude < speed)
            transform.position = MoveTarget;
        else
            transform.position += moveVector.normalized * speed;
    }

    // Mark unit as selected
    public void Select()
    {
        IsSelected = true;
    }

    // Unmark unit as selected
    public void Deselect()
    {
        IsSelected = false;
    }
}

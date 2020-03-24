using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private float movementSpeed; // in km/h

    [SerializeField]
    public float range; // in meters

    [HideInInspector]
    public bool IsSelected;

    [HideInInspector]
    public bool IsHovered;

    [HideInInspector]
    public Queue<UnitWaypoint> Waypoints = new Queue<UnitWaypoint>();

    private void FixedUpdate()
    {
        if (Waypoints.Count > 0)
            Move(Waypoints.Peek().Position);
    }

    private void Move(Vector3 moveTarget)
    {
        Vector3 moveVector = (moveTarget - transform.position);
        moveVector.y = 0;

        // Return if finished moving
        if (moveVector.magnitude == 0)
        {
            // Dequeue if moveTarget was a waypoint
            if (moveTarget == Waypoints.Peek().Position)
                Waypoints.Dequeue();
            return;
        }

        // Calculate speed in meters per fixed delta time
        float speed = movementSpeed / 3.6f * Time.fixedDeltaTime;

        // Move with speed up to movement speed
        if (moveVector.magnitude < speed)
            transform.position = moveTarget;
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

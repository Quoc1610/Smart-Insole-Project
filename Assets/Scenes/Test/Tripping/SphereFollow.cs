using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereFollow : MonoBehaviour
{
    public Transform target; // Reference to the target object
    private Vector3 originalOffset; // Original offset from the target
    private Vector3 currentOffset; // Current offset based on target rotation

    void Start()
    {
        if (target != null)
        {
            // Calculate the original offset based on the initial position difference
            originalOffset = transform.position - target.position;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // Calculate the target position in world space
            Vector3 targetPosition = target.position;

            // Calculate the desired position based on the target's rotation and distance, adjusting for offset
            Vector3 desiredPosition = targetPosition + currentOffset;

            // Move the object to maintain the desired position relative to the target's rotation
            transform.position = desiredPosition;

            // Update the current offset based on the target's rotation
            currentOffset = target.rotation * originalOffset;
        }
    }
}
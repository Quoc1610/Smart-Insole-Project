using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfGravity : MonoBehaviour
{
    private Rigidbody[] rigidBodies;  // Array of rigid bodies representing the skeleton components
    public GameObject bone;
    void Start()
    {
        rigidBodies = bone.GetComponentsInChildren<Rigidbody>();
        Debug.Log("Size: " + rigidBodies.Length);
        CalculateCenterOfGravity();
    }

    private void Update()
    {
        CalculateCenterOfGravity();
    }


    void CalculateCenterOfGravity()
    {
        Vector3 centerOfGravity = Vector3.zero;
        float totalMass = 0f;

        foreach (Rigidbody rb in rigidBodies)
        {
            centerOfGravity += rb.worldCenterOfMass * rb.mass;
            totalMass += rb.mass;
        }

        centerOfGravity /= totalMass;
        transform.position = centerOfGravity;
        transform.position = centerOfGravity;
    }
}

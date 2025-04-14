using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockLeg : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody[] rigidbodies;
    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            //rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

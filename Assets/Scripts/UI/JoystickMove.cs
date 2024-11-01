using UnityEngine;
using System;
using FIMSpace.RagdollAnimatorDemo;
using System.Collections.Generic;

public class JoystickMove : MonoBehaviour
{
    public Joystick movementJoystick;
    private Rigidbody rb;
    private Camera mainCamera;

    public FBasic_RigidbodyMover lsfBasic_RigidbodyMover;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(movementJoystick.Direction.x, 0, movementJoystick.Direction.y);
        Transform playerTransform = transform;
        direction = mainCamera.transform.TransformDirection(direction);
        direction.y = 0;

        if (direction.magnitude >= 0.1f)
        {
            //Debug.Log("Joystick: " + direction.ToString());
            //lsfBasic_RigidbodyMover.moveDirectionWorld = direction;
        }
        else
        {
            //Debug.Log("Joystick: " + Vector3.zero.ToString());
            //lsfBasic_RigidbodyMover.moveDirectionWorld = Vector3.zero;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RotationHandler : MonoBehaviour
{
    // Start is called before the first frame update
    public SetPressure pressure;
    public TestRotation Left;
    public TestRotation Right;

    public Transform objectA;
    public Transform objectB;

    private Vector3 oldPosA;
    private Vector3 oldPosB;


    float offset = 16.4f * -1;

    private Vector3 previousDirection;
    void Start()
    {
        oldPosA = objectA.position;
        oldPosB = objectB.position;

        previousDirection = (objectB.position - objectA.position).normalized;
    }
    public void UpdateRotation()
    {
        Vector3 currentDirection = (objectB.position - objectA.position).normalized;

        // Calculate the rotation needed from previous to current direction
        Quaternion rotationDelta = Quaternion.FromToRotation(previousDirection, currentDirection);

        // Apply that rotation to THIS object
        Left.transform.rotation = rotationDelta * Left.transform.rotation;
        Right.transform.rotation = rotationDelta * Right.transform.rotation;
        // Update the previous direction for next time
        previousDirection = currentDirection;
    }

    // Update is called once per frame
    public Quaternion GetRotationChange()
    {
        // Current direction
        Vector3 oldDirection = oldPosB - oldPosA;
        Vector3 newDirection = objectB.position - objectA.position;

        // If either vector is too small, avoid dividing by zero
        if (oldDirection.sqrMagnitude < 0.0001f || newDirection.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("Direction too small to calculate rotation.");
            return Quaternion.identity;
        }

        // Calculate rotation difference
        Quaternion rotationChange = Quaternion.FromToRotation(oldDirection.normalized, newDirection.normalized);

        // Update the old positions to the new ones for next time
        oldPosA = objectA.position;
        oldPosB = objectB.position;

        return rotationChange;
    }
    int count = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Move");
            SetPressure.SensorInfo sensor = pressure.getDebugSensor();
            Vector3 rotation = new Vector3(sensor.gyro["Left"][0] / offset, sensor.gyro["Left"][2] / offset, sensor.gyro["Left"][1] / offset);
            Left.changeRotation(rotation.x, rotation.y, rotation.z);
            Debug.Log(new Vector3(sensor.gyro["Left"][0] / offset, sensor.gyro["Left"][2] / offset, sensor.gyro["Left"][1] / offset));
            Left.ShowForce(new Vector3(sensor.accel["Left"][0], sensor.accel["Left"][1], sensor.accel["Left"][2]));
            Right.changeRotation(sensor.gyro["Right"][0] / offset, sensor.gyro["Right"][2] / offset, sensor.gyro["Right"][1] / offset);
            Right.ShowForce(new Vector3(sensor.accel["Right"][0], sensor.accel["Right"][1], sensor.accel["Right"][2]));
            //Vector3 angle = GetRotationChange().eulerAngles / Time.deltaTime;
            //Right.changeRotation(angle.x, 0, 0);
        }
    }
}

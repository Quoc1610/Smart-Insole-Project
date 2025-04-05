using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseOfSupport : MonoBehaviour
{
    public Transform leftFootBone;   // Transform of the left foot bone
    public Transform rightFootBone;  // Transform of the right foot bone

    private float width = 1.0f;   // Width of the cube
    private float height = 1.0f;  // Height of the cube
    public float scale = 1.0f;

    void Start()
    {
        CalculateBaseOfSupport();
        ResizeCubeObject();
    }

    private void Update()
    {
        ResizeCubeObject();
    }

    void CalculateBaseOfSupport()
    {

        // Get the positions of the left and right foot bones
        Vector3 leftFootPosition = leftFootBone.position;
        Vector3 rightFootPosition = rightFootBone.position;

        // Calculate the center of the base of support
        Vector3 center = (leftFootPosition + rightFootPosition) / 2f;

        // Calculate the width of the base of support
        width = Vector3.Distance(leftFootPosition, rightFootPosition);

        // Calculate the height of the base of support as the average of the left and right foot heights
        height = (leftFootPosition.y + rightFootPosition.y) / 2f;
        transform.position = center;
        Debug.Log("Base of Support - Center: " + center + ", Width: " + width + ", Height: " + height);
    }

    void ResizeCubeObject()
    {
        // Set the scale of the cube based on the width and height
        Vector3 newScale = new Vector3(width, scale, height);
        transform.localScale = newScale;

        // Adjust the position to keep the base in the XZ plane and the Y-axis constant
        Vector3 newPosition = new Vector3(transform.position.x, height / 2.0f, transform.position.z);
        transform.position = newPosition;
    }
}

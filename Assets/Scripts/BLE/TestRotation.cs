using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotation : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 rotationAdjustment = Vector3.zero;
    private LineRenderer lineRenderer;

    [Header("Line Settings")]
    public Color lineColor = Color.red;
    public float lineWidth = 0.05f;

    float avgScale()
    {
        return (transform.localScale.x + transform.localScale.y + transform.localScale.z) / 3f;
    }

    public void changeRotation(float x, float y, float z)
    {
        rotationAdjustment = new Vector3(x, y, z);
        transform.Rotate(rotationAdjustment * Time.deltaTime * 0.99f);
    }

    public void ShowForce(Vector3 force)
    {
        Vector3 startPoint = transform.position;
        Vector3 endPoint = startPoint + force * avgScale();

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }

    // Update is called once per frame
    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Line setup
        lineRenderer.positionCount = 2;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth * avgScale();
        lineRenderer.endWidth = lineWidth * avgScale();
    }
}

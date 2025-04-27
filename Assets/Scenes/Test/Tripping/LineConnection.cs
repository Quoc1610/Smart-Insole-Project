using UnityEngine;

public class LineConnection : MonoBehaviour
{
    public Transform endPoint;   // Transform of the second GameObject

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
        }
        lineRenderer.positionCount = 2;
        // Get the parent's scale
        if (transform.parent.parent != null)
        {
            Vector3 parentScale = transform.parent.parent.localScale;

            // Calculate the average scale factor of the parent
            float parentAverageScale = (parentScale.x + parentScale.y + parentScale.z) / 3f;

            // Scale the LineRenderer width based on the parent's average scale
            lineRenderer.startWidth *= parentAverageScale;
            lineRenderer.endWidth *= parentAverageScale;
        }
        
    }

    private void Update()
    {
        if (endPoint != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint.position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, endPoint.position);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
public class BaseOfSupport : MonoBehaviour
{
    [Header("Game Objects")]
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;

    [Header("Activation Bools")]
    public bool usePoint1 = false;
    public bool usePoint2 = false;
    public bool usePoint3 = false;
    public bool usePoint4 = false;

    [Header("Settings")]
    public float lineWidth = 0.5f;

    private Vector2[] vertices;

    public Transform CoG;
    public TextMeshProUGUI testtext;

    public BLEManager bleManager;

    void setText(string text)
    {
        if (testtext) testtext.text = text;
    }

    List<Transform> activePoints = new List<Transform>();
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        activePoints = new List<Transform>();
        if (usePoint1 && point1 != null) activePoints.Add(point1);
        if (usePoint2 && point2 != null) activePoints.Add(point2);
        if (usePoint3 && point3 != null) activePoints.Add(point3);
        if (usePoint4 && point4 != null) activePoints.Add(point4);

        int count = activePoints.Count;
        Gizmos.color = Color.cyan;

        if (count == 4)
        {
            DrawPolygon(activePoints);
        }
        else if (count == 3)
        {
            DrawPolygon(activePoints);
        }
        else if (count == 2)
        {
            DrawLineArea(activePoints[0].position, activePoints[1].position, lineWidth);
        }
        else if (count == 1)
        {
            //Gizmos.DrawWireSphere(activePoints[0].position, circleRadius);
            DrawLineArea(activePoints[0].position+Vector3.forward * (lineWidth / 2), activePoints[0].position + Vector3.back * (lineWidth / 2), lineWidth);
        }

        if (count != 0)
        {
            if (IsPointInPolygon(getPosition(CoG.position), vertices))
            {
                setText("Center of Gravity is inside Base of Suppport");
                if (bleManager != null) bleManager.PredictTrippingRisk(true);
            }
            else
            {
                setText("Center of Gravity is outside Base of Suppport");
                if (bleManager != null) bleManager.PredictTrippingRisk(false);
            }
        }
        else
        {
            Debug.Log("BoS: No");
            setText("No Base of Suppport");
        }
    }

    void DrawPolygon(List<Transform> points)
    {
        vertices = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 current = points[i].position;
            vertices[i] = getPosition(current);
            Vector3 next = points[(i + 1) % points.Count].position;
            Gizmos.DrawLine(current, next);
        }
    }
    Vector2 getPosition(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }

    void DrawLineArea(Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = end - start;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up) * (width / 2f);

        Vector3 p1 = start + perpendicular;
        Vector3 p2 = start - perpendicular;
        Vector3 p3 = end - perpendicular;
        Vector3 p4 = end + perpendicular;

        vertices = new Vector2[] { getPosition(p1), getPosition(p2), getPosition(p3), getPosition(p4) };

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }

    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length;
        bool isInside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
               (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    public void assginBool(float[] outputs)
    {
        //bool[] groundDetect = new bool[4];
        //for (int i = 0;i<4;i++)
        //{
        //    if (outputs[i] > 0.5) groundDetect[i] = true;
        //    else groundDetect[i] = false;
        //}

        //usePoint1 = groundDetect[2];
        //usePoint2 = !groundDetect[3];
        //usePoint3 = !groundDetect[1];
        //usePoint4 = groundDetect[0];

        bool[] groundDetect = new bool[2];
        for (int i = 0; i < 2; i++)
        {
            if (outputs[i] > 0.5) groundDetect[i] = true;
            else groundDetect[i] = false;
        }

        usePoint1 = groundDetect[1];
        usePoint2 = groundDetect[1];
        usePoint3 = groundDetect[0];
        usePoint4 = groundDetect[0];
    }
}
#else
public class BaseOfSupport : MonoBehaviour
{
    [Header("Game Objects")]
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;

    [Header("Activation Bools")]
    public bool usePoint1 = false;
    public bool usePoint2 = false;
    public bool usePoint3 = false;
    public bool usePoint4 = false;

    [Header("Settings")]
    public float lineWidth = 0.5f;

    private Vector2[] vertices;

    public Transform CoG;
    public TextMeshProUGUI testtext;

    public BLEManager bleManager;

    void setText(string text)
    {
        if (testtext) testtext.text = text;
    }

    void Update()
    {
        List<Transform> activePoints = new List<Transform>();

        if (usePoint1 && point1 != null) activePoints.Add(point1);
        if (usePoint2 && point2 != null) activePoints.Add(point2);
        if (usePoint3 && point3 != null) activePoints.Add(point3);
        if (usePoint4 && point4 != null) activePoints.Add(point4);

        int count = activePoints.Count;

        if (count == 4)
        {
            BuildPolygon(activePoints);
        }
        else if (count == 3)
        {
            BuildPolygon(activePoints);
        }
        else if (count == 2)
        {
            BuildLineArea(activePoints[0].position, activePoints[1].position, lineWidth);
        }
        else if (count == 1)
        {
            BuildLineArea(activePoints[0].position + Vector3.forward * (lineWidth / 2), activePoints[0].position + Vector3.back * (lineWidth / 2), lineWidth);
        }

        if (count != 0)
        {
            if (IsPointInPolygon(getPosition(CoG.position), vertices))
            {
                setText("Center of Gravity is inside Base of Support");
                if (bleManager != null) bleManager.PredictTrippingRisk(true);
            }
            else
            {
                setText("Center of Gravity is outside Base of Support");
                if (bleManager != null) bleManager.PredictTrippingRisk(false);
            }
        }
        else
        {
            setText("No Base of Support");
        }
    }

    void BuildPolygon(List<Transform> points)
    {
        vertices = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 current = points[i].position;
            vertices[i] = getPosition(current);
        }
    }

    Vector2 getPosition(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }

    void BuildLineArea(Vector3 start, Vector3 end, float width)
    {
        Vector3 direction = end - start;
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up) * (width / 2f);

        Vector3 p1 = start + perpendicular;
        Vector3 p2 = start - perpendicular;
        Vector3 p3 = end - perpendicular;
        Vector3 p4 = end + perpendicular;

        vertices = new Vector2[] { getPosition(p1), getPosition(p2), getPosition(p3), getPosition(p4) };
    }

    bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
    {
        int polygonLength = polygon.Length;
        bool isInside = false;

        for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
        {
            if (((polygon[i].y > point.y) != (polygon[j].y > point.y)) &&
               (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    public void assginBool(float[] outputs)
    {
        bool[] groundDetect = new bool[2];
        for (int i = 0; i < 2; i++)
        {
            groundDetect[i] = outputs[i] > 0.5f;
        }

        usePoint1 = groundDetect[1];
        usePoint2 = groundDetect[1];
        usePoint3 = groundDetect[0];
        usePoint4 = groundDetect[0];
    }
}
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockFoot : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform foot;
    private Vector3 position;
    private Quaternion rotation;
    private Vector3 scale;
    void Start()
    {
        position = foot.position;
        rotation = foot.rotation;
        scale = foot.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        foot.position = position;
        foot.rotation = rotation;
        foot.localScale = scale;
    }
}

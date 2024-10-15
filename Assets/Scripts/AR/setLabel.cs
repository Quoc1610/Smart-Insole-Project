using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class applyLabel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Character";
        Debug.Log("!Testing");
        DisableAndReplaceObjectsByTag("ARPlane");
    }
    private void DisableAndReplaceObjectsByTag(string tag)
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objectsWithTag)
        {
            obj.SetActive(false);
        }
    }
}

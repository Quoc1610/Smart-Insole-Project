using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        float currentFPS = 1f / Time.deltaTime;
        if (fpsText) fpsText.text = "FPS: " + Mathf.Round(currentFPS);
    }
}

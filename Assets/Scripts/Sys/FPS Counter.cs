using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI fpsText;

    private void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        float currentFPS = 1f / Time.deltaTime;
        fpsText.text = "FPS: " + Mathf.Round(currentFPS);
    }
}

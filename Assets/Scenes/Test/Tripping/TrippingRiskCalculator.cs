using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TrippingRiskCalculator : MonoBehaviour
{
    // Adjustable parameters
    [Range(0.01f,0.1f)] public float steepness = 0.01f; // k

    // Test values
    private float momentum = 0.0f; // x
    private float friction = 0.0f; // y

    [Range(40, 120)] public float weight = 60f; // kg
    [Range(0, 20)] public float speed = 2f; // m/s

    [Range(0, 3000)] public float footPressure = 200f; // mbar
    [Range(0, 1)] public float footPressureScale = 0.25f;

    public bool isBalanced = false;

    public Image image;
    public TextMeshProUGUI risk;
    float result = 0;

    void Update()
    {
        momentum = weight * speed;
        friction = footPressure * footPressureScale;
        if (isBalanced) result = 0;
        else result = CalculateActivation(momentum, friction, steepness);
        UpdateBar(result);
        if (risk)
        {
            string text = "Low";
            Color color = Color.white;
            if (result < .33f)
            {
                text = "Low";
                color = Color.yellow;
            }
            else if (result < .66f)
            {
                text = "Medium";
                color = new Color32(255, 165, 0, 255); // Orange
            }
            else
            {
                text = "High";
                color = Color.red;
            }
            risk.text = "Risk: " + text;
            risk.color = color;
        }
    }

    /// <summary>
    /// Computes the logistic-based comparison function.
    /// </summary>
    float CalculateActivation(float momentum, float friction, float k)
    {
        //float delta = momentum - friction - m;
        //float logistic = 1f / (1f + Mathf.Exp(-k * delta));
        //float baseline = 1f / (1f + Mathf.Exp(k * m));
        //return Mathf.Max(0f, logistic - baseline);

        float delta = momentum - friction;
        float logistic = 1f / (1f + Mathf.Exp(-k * delta));
        return Mathf.Max(0f, logistic);
    }

    void UpdateBar(float amount)
    {
        if (image != null)
        {
            image.fillAmount = amount;
        }
    }
}

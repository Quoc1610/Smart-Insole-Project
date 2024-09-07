using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public TextMeshProUGUI outputBMI;
    public TMP_InputField inputHeight;      // in cm
    public TMP_InputField inputWeight;      // in kg
    public Slider sliderHeight;
    public Slider sliderWeight;


    public void ButtonSubmit()
    {

    }

    private void CalculateBMI()
    {
        double height = Convert.ToDouble(inputHeight.text) / 100;
        double weight = Convert.ToDouble(inputWeight.text);

        double BMI = weight / (height * height) ;

        outputBMI.text = "Your BMI: " + Math.Round(BMI, 1).ToString();
    }

    private void HandleSlider()
    {
        inputHeight.text = sliderHeight.value.ToString();
        inputWeight.text = sliderWeight.value.ToString();
    }

    // Start is called before the first frame update
    void Awake()
    {
        sliderHeight.maxValue = 300;    // cm
        sliderHeight.minValue = 100;    // cm
        sliderWeight.maxValue = 200;    // kg
        sliderWeight.minValue = 30;     // kg

        inputHeight.text = "0";
        inputWeight.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        CalculateBMI();
        HandleSlider();
    }
}

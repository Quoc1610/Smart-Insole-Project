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

    double height;
    double weight;
    double BMI;


    public void ButtonSubmit()
    {

    }

    private void CalculateBMI()
    {
        height = Convert.ToDouble(inputHeight.text) / 100;
        weight = Convert.ToDouble(inputWeight.text);

        BMI = weight / (height * height) ;

        outputBMI.text = "Your BMI: " + Math.Round(BMI, 1).ToString();
    }
    public void ChangeValueSlider(int index){
        if(index == 0)
        {
            sliderHeight.value = Convert.ToSingle(inputHeight.text);
        }
        else if(index == 1)
        {
            sliderWeight.value = Convert.ToSingle(inputWeight.text);
        }
    }
    public void HandleSlider(int index)
    {
        if(index == 0)
        {
            inputHeight.text = sliderHeight.value.ToString();
        }
        else if(index == 1)
        {
            inputWeight.text = sliderWeight.value.ToString();
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        weight = 0;
        height = 0;
        BMI = 0;

        sliderHeight.maxValue = 300;    // cm
        sliderHeight.minValue = 100;    // cm
        sliderWeight.maxValue = 200;    // kg
        sliderWeight.minValue = 30;     // kg
    }

    // Update is called once per frame
    void Update()
    {
        CalculateBMI();
        // HandleSlider();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

public class UIScript : MonoBehaviour
{
    public TextMeshProUGUI outputBMI;
    public TMP_InputField inputHeight;      // in cm
    public TMP_InputField inputWeight;      // in kg
    public Slider sliderHeight;
    public Slider sliderWeight;
    public GameObject model;

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

        // Average Height of men is 170cm (scale y = 1)
        // scale y = height / avg height
        model.transform.localScale = new Vector3((float)(weight * 0.03), (float)(height / 1.7), 3);

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

        sliderHeight.maxValue = 200;    // cm
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class UIInApp : MonoBehaviour
{
    // Start is called before the first frame updat
    public List<Button> btnPopUp=new List<Button>();
    public GameObject goPopUp;
    public Slider sliderSpeed;
    public Slider sliderStepLength;
    public Slider sliderScale;
    public TextMeshProUGUI txtSpeed;
    public List<GameObject> lsGOChar=new List<GameObject>();
    public Vector3 v3CharScale;
    public TextMeshProUGUI txtStepLength;
    public int indexgoChar;
    public void OnSetUp(){
        btnPopUp[0].gameObject.SetActive(true);
        btnPopUp[1].gameObject.SetActive(false);
        goPopUp.SetActive(false);
        sliderSpeed.maxValue=1;
        sliderSpeed.minValue=0;
        sliderStepLength.maxValue=1;
        sliderStepLength.minValue=0;
        sliderScale.maxValue=1;
        sliderScale.minValue=0;
    }
    public void OnSetUpScale(){
        v3CharScale=lsGOChar[indexgoChar].transform.localScale;
    }
    public void OnSliderSpeedChange()
    {
        //todo
    }
    public void OnSliderStepChange()
    {
        //todo
    }
    public void OnSliderScaleChange()
    {
        
        lsGOChar[indexgoChar].transform.localScale=v3CharScale;
        Debug.Log("v3CharScale: "+v3CharScale);
        Debug.Log("lsGOChar[indexgoChar].transform.localScale: "+lsGOChar[indexgoChar].transform.localScale);
        lsGOChar[indexgoChar].transform.localScale= lsGOChar[indexgoChar].transform.localScale*(1+sliderScale.value);
    }
    public void OnBtnPopUpClick(int index){
        if(index==0){
            btnPopUp[0].gameObject.SetActive(false);
            btnPopUp[1].gameObject.SetActive(true);
            goPopUp.SetActive(true);
        }
        else{
            btnPopUp[0].gameObject.SetActive(true);
            btnPopUp[1].gameObject.SetActive(false);
            goPopUp.SetActive(false);
        }
    }
}

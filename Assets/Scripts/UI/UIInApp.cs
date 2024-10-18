using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using FIMSpace;
using FIMSpace.RagdollAnimatorDemo;

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
    public List<FBasic_RigidbodyMover> lsfBasic_RigidbodyMover=new List<FBasic_RigidbodyMover>();

    public int indexgoChar;
    public void OnSetUp(){
        btnPopUp[0].gameObject.SetActive(true);
        btnPopUp[1].gameObject.SetActive(false);
        goPopUp.SetActive(false);
        sliderSpeed.maxValue=5;
        sliderSpeed.minValue=1;
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
        txtSpeed.text="Speed: "+sliderSpeed.value.ToString("F2");
        lsfBasic_RigidbodyMover[indexgoChar].speedAnim=sliderSpeed.value;
    }
    public void OnSliderStepChange()
    {
        //todo
        txtStepLength.text="Step Length: "+sliderStepLength.value.ToString("F2");
        lsfBasic_RigidbodyMover[indexgoChar].stepLength=sliderStepLength.value;
    }
    public void OnSliderScaleChange()
    {
        
        lsGOChar[indexgoChar].transform.localScale=v3CharScale;
        Debug.Log("v3CharScale: "+v3CharScale);
        Debug.Log("lsGOChar[indexgoChar].transform.localScale: "+lsGOChar[indexgoChar].transform.localScale);
        lsGOChar[indexgoChar].transform.localScale= lsGOChar[indexgoChar].transform.localScale*(1+sliderScale.value);
    }

    public void OnBtnHomeClick()
    {
        // TODO
        // Go back to main menu
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

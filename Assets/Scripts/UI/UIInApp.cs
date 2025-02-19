using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using FIMSpace;
using FIMSpace.RagdollAnimatorDemo;
using JetBrains.Annotations;

public class UIInApp : MonoBehaviour
{
    // Start is called before the first frame updat
    public List<Button> btnPopUp=new List<Button>();
    public GameObject goPopUp;
    public Slider sliderSpeed;
    public Slider sliderMoveSpeed;
    public Slider sliderStepLength;
    public List<Button> btnScale=new List<Button>();
    public List<GameObject> lsgoBorder=new List<GameObject>();
    public List<GameObject> lsgoTab=new List<GameObject>();
    public TextMeshProUGUI txtSpeed;
    public TextMeshProUGUI txtMoveSpeed;    
    public List<GameObject> lsGOChar=new List<GameObject>();
    public Vector3 v3CharScale;
    public TextMeshProUGUI txtStepLength;
    public TextMeshProUGUI txtScale;
    public List<FBasic_RigidbodyMover> lsfBasic_RigidbodyMover=new List<FBasic_RigidbodyMover>();
    public GameObject ShowData;
    bool isShowData = false;

    public int indexgoChar;
    public void OnSetUp(){
        btnPopUp[0].gameObject.SetActive(true);
        btnPopUp[1].gameObject.SetActive(false);
        goPopUp.SetActive(false);
        sliderSpeed.maxValue=5;
        sliderSpeed.minValue=1;
        sliderStepLength.maxValue=1;
        sliderStepLength.minValue=0;
        sliderMoveSpeed.maxValue=5;
        sliderMoveSpeed.minValue=2;
        lsgoBorder[0].SetActive(true);
        lsgoBorder[1].SetActive(false);
        lsgoBorder[2].SetActive(false);
        lsgoTab[0].SetActive(true);
        lsgoTab[1].SetActive(false);
        lsgoTab[2].SetActive(false);
        ShowData.SetActive(isShowData);
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
    public void OnSliderMoveSpeedChange()
    {
        //todo
        txtMoveSpeed.text="Move Speed: "+sliderMoveSpeed.value.ToString("F2");
        lsfBasic_RigidbodyMover[indexgoChar].MovementSpeed=sliderMoveSpeed.value;
    }
    public void OnSetUpScale(){
        v3CharScale=lsGOChar[indexgoChar].transform.localScale;
    }
    public void OnbtnScaleClick(int index)
    {
       
        if(index==0){
            if(lsGOChar[indexgoChar].transform.localScale.x/v3CharScale.x==1f){
                return;
            }
            lsGOChar[indexgoChar].transform.localScale-=v3CharScale;
        }
        else{
            lsGOChar[indexgoChar].transform.localScale+=v3CharScale;
        }
        UpdateScaleText();
    }
    public void UpdateScaleText(){
        txtScale.text=(lsGOChar[indexgoChar].transform.localScale.x/v3CharScale.x).ToString("F2");
    }
    public void OnBtnHomeClick()
    {
        // TODO
        // Go back to main menu
        UIManager._instance.uiMainMenu.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
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
    public void OnBtnTabClick(int index){

        //if(index==0){
        //    lsgoBorder[0].SetActive(true);
        //    lsgoBorder[1].SetActive(false);
        //    lsgoTab[0].SetActive(true);
        //    lsgoTab[1].SetActive(false);

        //}
        //else{
        //    lsgoBorder[0].SetActive(false);
        //    lsgoBorder[1].SetActive(true);
        //    lsgoTab[0].SetActive(false);
        //    lsgoTab[1].SetActive(true);
        //}
        for (int i = 0; i < lsgoBorder.Count; i++)
        {
            lsgoBorder[i].SetActive(false);
            lsgoTab[i].SetActive(false);
        }
        lsgoBorder[index].SetActive(true);
        lsgoTab[index].SetActive(true);

    }

    public void toggleData()
    {
        isShowData = !isShowData;
        ShowData.SetActive(isShowData);
    }
}



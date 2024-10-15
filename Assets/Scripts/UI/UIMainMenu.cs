using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public Button btnImgTarget;
    public Button btnPlain;
    public GameObject goImgTarget;
    public GameObject goPlain;


    public void OnSetUp()
    {
        goImgTarget.SetActive(false);
        goPlain.SetActive(false);
    }
    public void OnBtnImgTarget_Clicked()
    {
        goImgTarget.SetActive(true);
        goPlain.SetActive(false);
    }

    public void OnBtnPlainTarget_Clicked()
    {
        goImgTarget.SetActive(false);
        goPlain.SetActive(true);
    }

    void Start()
    {
        OnSetUp();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

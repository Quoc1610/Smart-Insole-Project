using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        UIManager._instance.goJoystick.SetActive(false);
    }
    public void OnBtnImgTarget_Clicked()
    {
        goImgTarget.SetActive(true);
        goPlain.SetActive(false);
        UIManager._instance.uiInApp.indexgoChar = 0;
        UIManager._instance.uiInApp.lsGOChar[0].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        UIManager._instance.uiInApp.OnSetUpScale();
        UIManager._instance.goJoystick.SetActive(true);
        UIManager._instance.uiController.mode = 1;
        UIManager._instance.uiController.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
        UIManager._instance.uiInApp.gameObject.SetActive(true);

    }

    public void OnBtnPlainTarget_Clicked()
    {
        goImgTarget.SetActive(false);
        goPlain.SetActive(true);
        UIManager._instance.uiInApp.indexgoChar = 1;
        UIManager._instance.uiInApp.lsGOChar[1].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        UIManager._instance.uiInApp.OnSetUpScale();
        UIManager._instance.goJoystick.SetActive(true);
        UIManager._instance.uiController.mode = 0;
        UIManager._instance.uiController.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
        UIManager._instance.uiInApp.gameObject.SetActive(true);

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

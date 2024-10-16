using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UIMainMenu uiMainMenu;
    public UIInApp uiInApp;
    public GameObject goJoystick;
    public static UIManager _instance { get; private set; }
    private void Awake() {
         if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
        uiInApp.gameObject.SetActive(false);
        uiInApp.OnSetUp();
        goJoystick.SetActive(false);
        
    }
    public void OnLoadGameScene()
    {
        uiMainMenu.gameObject.SetActive(false);
    }
}
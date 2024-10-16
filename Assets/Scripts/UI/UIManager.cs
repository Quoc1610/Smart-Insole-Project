using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UIMainMenu uiMainMenu;
    public GameObject goJoystick;
    public static UIManager _instance { get; private set; }
    private void Start() {
        _instance = this;
        goJoystick.SetActive(false);
        
    }
    public void OnLoadGameScene()
    {
        uiMainMenu.gameObject.SetActive(false);
    }
}
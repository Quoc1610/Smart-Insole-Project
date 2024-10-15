using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UIMainMenu uiMainMenu;
    public UIInApp uiInApp;
    public static UIManager _instance { get; private set; }
    public void OnLoadGameScene()
    {
        uiMainMenu.gameObject.SetActive(false);
        uiInApp.gameObject.SetActive(true);
    }
    private void Start() {
        OnLoadGameScene();
        uiInApp.OnSetUp();
    }
}
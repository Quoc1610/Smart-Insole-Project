using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unisave.EmailAuthentication;
using Unisave.Facets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPageController : MonoBehaviour
{
    // Start is called before the first frame update
    private UserInstance userInstance;
    public TextMeshProUGUI steps;
    public TextMeshProUGUI distance;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI prevents;

    public Button account;
    bool isSlider = false;
    public Button quit;
    public Button setting;
    public GameObject slider;
    public Button planePutDown;

    void Start()
    {
        userInstance = GameObject.FindGameObjectWithTag("UserInstance").GetComponent<UserInstance>();
        slider.SetActive(isSlider);
        printDashBoard();
        account.onClick.AddListener(OnAccountClick);
        quit.onClick.AddListener(OnLogoutClick);
        planePutDown.onClick.AddListener(OnGroundBasePlaneClick);
    }

    // Update is called once per frame
    void Update()
    {
        printDashBoard();
    }

    void OnAccountClick()
    {
        isSlider = !isSlider;
        slider.SetActive(isSlider);
    }

    private void OnLogoutClick()
    {
        // logout in unisave
        this.CallFacet((EmailAuthFacet f) => f.Logout());
        SceneManager.LoadScene("Login");
    }

    private void OnGroundBasePlaneClick()
    {
        SceneManager.LoadScene("AR 1");
    }

    void printDashBoard()
    {
        if (userInstance != null)
        {
            steps.text = userInstance.entity.totalSteps.ToString();
            distance.text = userInstance.entity.totalDistance.ToString();
            speed.text = userInstance.entity.averageSpeed.ToString();
            prevents.text = userInstance.entity.totalPrevent.ToString();
        }
    }
}

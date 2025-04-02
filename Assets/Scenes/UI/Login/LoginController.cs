using System;
using TMPro;
using Unisave.EmailAuthentication;
using Unisave.Examples.EmailAuthentication.Backend;
using Unisave.Facets;
using Unisave.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    // references to UI components
    public EmailAuthPanel authPanel;
    public Button quitButton;
    public UserInstance user;


    private void Start()
    {
        CheckRequiredDependencies();

        // start the game when a user logs in or registers
        authPanel.onLoginSuccess.AddListener(
            loginResponse => ShowMainPage()
        );
        authPanel.onRegistrationSuccess.AddListener(
            registerResponse => ShowMainPage()
        );

        // handle logout
        quitButton.onClick.AddListener(OnQuitClicked);

    }

    private void CheckRequiredDependencies()
    {
        if (authPanel == null)
            throw new ArgumentException(
                $"Link the '{nameof(authPanel)}' in the inspector."
            );

        
        if (quitButton == null)
            throw new ArgumentException(
                $"Link the '{nameof(quitButton)}' in the inspector."
            );

    }

    public void ShowAuthPanel()
    {
        authPanel.gameObject.SetActive(true);
        authPanel.ShowLoginForm();
    }

    public async void ShowMainPage()
    {
        // download player entity
       UserEntity entity = await this.CallFacet(
            (UserDataFacet f) => f.DownloadLoggedInPlayer()
        );
        user.getEntity(entity);
        SceneManager.LoadScene("MainPage");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
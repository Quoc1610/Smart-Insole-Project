using System.Collections;
using System.Collections.Generic;
using Unisave.Examples.EmailAuthentication.Backend;
using Unisave.Facets;
using UnityEngine;

public class UserInstance : MonoBehaviour
{
    // Start is called before the first frame update
    bool isLogin = false;
    public UserEntity entity;

    float timeoff = .5f;
    float currentTime = 0;

    public void getEntity(UserEntity _entity)
    {
        isLogin = true;
        entity = _entity;
    }
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > timeoff)
        {
            currentTime = 0;
            RefreshUser();
        }
    }

    async void RefreshUser()
    {
        if (!isLogin) return;
        // download player entity
        entity = await this.CallFacet(
            (UserDataFacet f) => f.DownloadLoggedInPlayer()
        );
    }

    public async void OnSaveSetting(float weight, float height, float bmi, float sex)
    {
        entity = await this.CallFacet(
            (UserDataFacet f) => f.SaveUserSetting(weight, height, bmi, sex)
        );
    }

    public async void OnSaveDashboard(int total_steps, float total_distance, float avg_speed, float avg_step_length)
    {
        entity = await this.CallFacet(
            (UserDataFacet f) => f.SaveUserDashBoard(total_steps, total_distance, avg_speed, avg_step_length)
        );
    }
}

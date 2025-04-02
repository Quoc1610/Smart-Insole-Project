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
}

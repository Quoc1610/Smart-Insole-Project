using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowImage : MonoBehaviour
{
    // Start is called before the first frame update
    public Button minButton;
    public Button maxButton;
    public GameObject showImage;
    void Start()
    {
        OnMaxButtonClick();
        minButton.onClick.AddListener(OnMinButtonClick);
        maxButton.onClick.AddListener(OnMaxButtonClick);
    }

    void OnMinButtonClick()
    {
        showImage.SetActive(true);
        minButton.gameObject.SetActive(false);
    }

    void OnMaxButtonClick()
    {
        showImage.SetActive(false);
        minButton.gameObject.SetActive(true);
    }
}

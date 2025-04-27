using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserSetting : MonoBehaviour
{
    // Start is called before the first frame update
    private UserInstance userInstance;

    public Button returnButton;
    public TMP_Dropdown sexField;
    public TMP_InputField weightField;
    public TMP_InputField heightField;
    public TextMeshProUGUI bmiField;
    public Button saveButton;
    public Button resetButton;

    public TextMeshProUGUI warning;

    void Start()
    {
        userInstance = GameObject.FindGameObjectWithTag("UserInstance").GetComponent<UserInstance>();
        Reset();
        warning.gameObject.SetActive(false);
        returnButton.onClick.AddListener(OnClickReturn);
        saveButton.onClick.AddListener(Save);
        resetButton.onClick.AddListener(Reset);
    }

    private void Reset()
    {
        weightField.text = userInstance.entity.weight.ToString();
        heightField.text = userInstance.entity.height.ToString();
        sexField.value = (int)userInstance.entity.sex;
        sexField.RefreshShownValue();
    }

    void Check()
    {
        float weight;
        float height;

        // Check if the weightField text can be parsed to a float number
        if (float.TryParse(weightField.text, out weight))
        {
            if (weight <= 0)
            {
                warning.text = "Weight must be greater than 0.";
                warning.gameObject.SetActive(true);
                return;
            }
        }
        else
        {
            warning.text = "Weight must be a number";
            warning.gameObject.SetActive(true);
            return;
        }

        // Check if the heightField text can be parsed to a float number
        if (float.TryParse(heightField.text, out height))
        {
            if (height <= 0)
            {
                warning.text = "Height must be greater than 0.";
                warning.gameObject.SetActive(true);
                return;
            }
        }
        else
        {
            warning.text = "Height must be a number";
            warning.gameObject.SetActive(true);
            return;
        }

        float bmi = weight / (height * .01f * height * 0.01f);
        bmiField.text = bmi.ToString("F2");
        warning.gameObject.SetActive(false);
    }

    void Save()
    {
        float weight, height, bmi, sex;
        float.TryParse(weightField.text, out weight);
        float.TryParse(heightField.text, out height);
        float.TryParse(bmiField.text, out bmi);
        sex = sexField.value;

        userInstance.OnSaveSetting(weight, height, bmi, sex);
    }

    void OnClickReturn()
    {
        Reset();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Check();
    }
}

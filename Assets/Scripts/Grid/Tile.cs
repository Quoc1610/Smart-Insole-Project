using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : Button
{
    public float width;
    public float height;
    public int gridX; 
    public int gridY; 
    public int realValue;
    private TextMeshProUGUI txtValue ;
    private UIPressure parentScript;

    public void OnSetUp(int x, int y, UIPressure parent)
    {
        image = GetComponent<Image>();
        realValue = 0;
        txtValue = GetComponentInChildren<TextMeshProUGUI>();
        width = this.gameObject.GetComponent<RectTransform>().rect.width;
        height = this.gameObject.GetComponent<RectTransform>().rect.height;
        txtValue.text = "0";
        gridX = x;
        gridY = y;
        parentScript = parent;
        onClick.AddListener(() => OnClicked(10));
    }

    public void OnClicked(int value)
    {
        if(realValue == 100)
        {
            return;
        }
        realValue = Mathf.Clamp(realValue + value, 0, 100); 
        if(realValue>=100)
        {
            realValue = 100;
        }
        txtValue.text = realValue.ToString();
        image.color = GetColorBasedOnValue(realValue);
        parentScript.ActOnNeighbors(gridX, gridY, 10,value);
    }
    public void UpdateValue(int value)
    {
        if(value >= 100)
        {
            value = 100;
        }
        realValue = value;
        txtValue.text = realValue.ToString();
        image.color = GetColorBasedOnValue(realValue);
    }
    public Color GetColorBasedOnValue(int Invalue)
    {
        float value = Invalue / 100f;
        if (value <= 0 || value > 1)
        {
            Debug.LogWarning("Value should be between 0 and 1.");
            return Color.white; 
        }

        if (value <= 0.5f)
        {
            float t = value / 0.5f;
            return new Color(0, t, 1 - t); 
        }
        else if (value <= 0.75f)
        {

            float t = (value - 0.5f) / 0.25f;
            return new Color(t, 1, 0); 
        }
        else
        {
            float t = (value - 0.75f) / 0.25f; 
            return new Color(1, 1 - t, 0);
        }
    }
}

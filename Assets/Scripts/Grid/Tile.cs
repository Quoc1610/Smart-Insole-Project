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
        onClick.AddListener(() => OnClicked(20));
    }

    public void OnClicked(int value)
    {
        realValue = Mathf.Clamp(realValue + value, 0, 100); 
        txtValue.text = realValue.ToString();
        image.color = GetColorBasedOnValue(realValue);
        parentScript.ActOnNeighbors(gridX, gridY, 5,value);
    }
    public void UpdateValue(int value)
    {
        realValue = value;
        txtValue.text = realValue.ToString();
        image.color = GetColorBasedOnValue(realValue);
    }
    public Color GetColorBasedOnValue(int value)
    {
        if(value==0){
            return Color.white;
        }
        else if (value <= 25)
        {
            return Color.blue;
        }
        else if (value <= 50)
        {
            return Color.green;
        }
        else if (value <= 75)
        {
            return Color.yellow;
        }
        else
        {
            return Color.red;
        }
    }
}

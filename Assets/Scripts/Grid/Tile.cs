using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile
{
    public float width;
    public float height;
    public int gridX; 
    public int gridY; 
    public int realValue;
    public int side;
    private UIPressure parentScript;

    public void OnSetUp(int x, int y, UIPressure parent,int side)
    {
        realValue = 0;
        gridX = x;
        gridY = y;
        parentScript = parent;
        this.side = side;
    }

    public void OnClicked(int value,int side)
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

        parentScript.ActOnNeighbors(gridX, gridY, 10,value,side);
       // UIManager._instance.uiPressure.textDebug.text+="X: "+gridX+" Y: "+gridY;
    }
    public void UpdateValue(int value)
    {
        if(value >= 100)
        {
            value = 100;
        }
        realValue = value;
    }
    public Color GetColorBasedOnValue(int Invalue)
    {
        float value = Invalue / 100f;
        if (value == -1)
        {
            return Color.grey;
        }
        if (value <= 0 || value > 1)
        {
            
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryCheck : MonoBehaviour
{
    // Start is called before the first frame update
    public Image battery;

    private void Start()
    {
        setBattery(0);
    }

    public void setBattery(float power)
    {
        if (power < 15) battery.color = Color.red;
        else battery.color = Color.green;
        battery.fillAmount = power / 100f;
    }
}

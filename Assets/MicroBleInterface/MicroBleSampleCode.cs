
//using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MicroBleSampleCode : MonoBehaviour
{
    public float accelx;
    public float accely;
    public float accelz;

    private MicroBleLib m_MicroBleLib;

    TMP_InputField inputFieldrow;
    TMP_InputField inputFieldcol;

    void Start()
    {
        this.transform.localScale = new Vector3(2, 2, 2);

        UnityEngine.Debug.LogWarning("start");
        //m_MicroBleLib = gameObject.AddComponent< MicroBleLib - v2 > ();
        m_MicroBleLib = gameObject.AddComponent<MicroBleLib>();
        m_MicroBleLib.MicroBleStart();

        inputFieldrow = GameObject.Find("InputFieldRow").GetComponent<TMP_InputField>();
        inputFieldcol = GameObject.Find("InputFieldCol").GetComponent<TMP_InputField>();
        inputFieldrow.text = "0";
        inputFieldcol.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        byte[] readdata = new byte[] { };
        //UnityEngine.Debug.LogWarning("Update");
        if (!m_MicroBleLib.UpdateRead(ref readdata))
        {
            return;
        }
        UnityEngine.Debug.LogWarning(" Read1: " + readdata[0] + " " + readdata[1] + " " + readdata[2]); 
        UnityEngine.Debug.LogWarning(" Read1: " + readdata.Length);

        string text = System.Text.Encoding.UTF8.GetString(readdata);
        UnityEngine.Debug.LogWarning(" Read: " + text);
        string[] arr = text.Split(',');
        float[] acceldata = new float[3];
        acceldata[0] = float.Parse(arr[0]) / 100000;
        acceldata[1] = float.Parse(arr[1]) / 100000;
        acceldata[2] = float.Parse(arr[2]) / 100000;


        UnityEngine.Debug.LogWarning(" Update: " + acceldata[0] + " " + acceldata[1] + " " + acceldata[2]);

        accelx = acceldata[0] * 400;
        accely = acceldata[1] * 400;
        accelz = acceldata[2] * 400;

        transform.rotation = Quaternion.AngleAxis(accelx, Vector3.up) * Quaternion.AngleAxis(accely, Vector3.right);
    }

    private void OnApplicationQuit()
    {
        UnityEngine.Debug.LogWarning("OnApplicationQuit");
        m_MicroBleLib.Quit();
    }

    public void ButtonClick()
    {
        UnityEngine.Debug.LogWarning("ButtonClick: ");

        byte[] writedata = new byte[2] { byte.Parse(inputFieldrow.text) , byte.Parse(inputFieldcol.text) };
        UnityEngine.Debug.LogWarning(writedata[0] + " " + writedata[1]);
        m_MicroBleLib.Command(writedata);
    }

}

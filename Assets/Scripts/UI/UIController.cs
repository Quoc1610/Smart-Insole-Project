using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public int mode = 0;
    public GameObject[] placement;
    public Button[] buttons; // Array to store references to all buttons
    private bool[] buttonStatus; // Array to store the status of each button

    void Start()
    {
        // Initialize the buttonStatus array to track the status of each button
        buttonStatus = new bool[buttons.Length];
    }

    public void OnButtonDown(int index)
    {
        Debug.Log("Clicked " + index.ToString());
        buttonStatus[index] = true; // Set the flag to true when the button is clicked and held
    }

    public void OnButtonUp(int index)
    {
        Debug.Log("Released " + index.ToString());
        buttonStatus[index] = false; // Set the flag to false when the button is released
    }

    // Check the value of the button status (true for held, false for released)
    public bool GetButtonValue(int index)
    {
        return buttonStatus[index];
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Move the placement object based on button input
    private void MovePlacementObject()
    {
        Vector3 movement = Vector3.zero;
        // Move the object left or right based on button input
        if (GetButtonValue(1))
        {
            movement += new Vector3(-1, 0, 0);
        }
        else if (GetButtonValue(3))
        {
            movement += new Vector3(1, 0, 0);
        }

        // Move the object forward or backward based on button input
        if (GetButtonValue(0))
        {
            movement += new Vector3(0, 0, 1);
        }
        else if (GetButtonValue(2))
        {
            movement += new Vector3(0, 0, -1);
        }

        // Move the object up or down based on button input
        if (GetButtonValue(4))
        {
            movement += new Vector3(0, 1, 0);
        }
        else if (GetButtonValue(5))
        {
            movement += new Vector3(0, -1, 0);
        }
        movement.Normalize();
        placement[mode].transform.localPosition += movement * Time.deltaTime;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonMovement : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;
    public UIController controller;
    public void OnPointerUp(PointerEventData eventData)
    {
        controller.OnButtonUp(value);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.OnButtonDown(value);
    }
}

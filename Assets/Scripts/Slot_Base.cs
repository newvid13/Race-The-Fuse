using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot_Base : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Vector2 position;
    public Quaternion rot; // new
    public int x;
    public int y;

    public GameObject myCard;
    public Image highlight;

    //public Action<int, int> actionAmCLicked;
    public event SlotClicked amClicked;
    public event SlotHover amHovered;
    bool isActive;

    public void SetupValues(int x, int y, Vector2 pos, Quaternion rot)
    {
        this.x = x;
        this.y = y;
        this.position = pos;
        this.rot = rot;

        GetComponent<RectTransform>().anchoredPosition = position;
        GetComponent<RectTransform>().rotation = rot;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        amClicked?.Invoke(x, y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isActive)
            amHovered?.Invoke(x, y, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isActive)
            amHovered?.Invoke(x, y, false);
    }

    public void ToggleHighlight(bool state)
    {
        if(state)
        {
            isActive = true;
            highlight.enabled = true;
        }
        else
        {
            isActive = false;
            highlight.enabled = false;
            amClicked = null;
            amHovered = null;
        }
    }
}

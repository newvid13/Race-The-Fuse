using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Button : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image clickedOn;
    [SerializeField] Image clickedOff;
    [SerializeField] AudioClip clipClicked;

    public void OnPointerDown(PointerEventData eventData)
    {
        //
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Manager_Menu.Audio.PlaySound(clipClicked);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {

    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {

    }

    public virtual void ChangeIcon(bool isOn)
    {
        if (!clickedOn || !clickedOff)
            return;

        if(isOn)
        {
            clickedOn.enabled = true;
            clickedOff.enabled = false;
        }
        else
        {
            clickedOff.enabled = true;
            clickedOn.enabled = false;
        }
    }
}

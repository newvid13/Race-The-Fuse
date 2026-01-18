using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button_Level : UI_Button
{
    [SerializeField] Level myLevel;
    [SerializeField] CanvasGroup infoGroup;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text difficultyText;
    [SerializeField] TMP_Text descriptionText;

    private void Start()
    {
        nameText.text = myLevel.levelName;
        difficultyText.text = myLevel.levelDifficulty;
        descriptionText.text = myLevel.levelDescription;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        Manager_Menu.instance.StartLevel(myLevel.sceneName);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        nameText.text = myLevel.levelName;
        difficultyText.text = myLevel.levelDifficulty;
        descriptionText.text = myLevel.levelDescription;

        Vector2 infoPos = GetComponent<RectTransform>().anchoredPosition;
        infoPos.x += 32;
        infoPos.y += 48;
        infoGroup.GetComponent<RectTransform>().anchoredPosition = infoPos;
        infoGroup.alpha = 1f;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        infoGroup.alpha = 0f;
    }

}

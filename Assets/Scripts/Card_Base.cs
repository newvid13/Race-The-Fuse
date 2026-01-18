using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Card_Base : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CardState CardState { get; set; }
    public int Attack {  get; private set; }
    public int Defense {  get; private set; }

    public Slot_Base MySlot { get; set; }

    [HideInInspector] public CardData cardData;
    public CardEffects cardEffects;

    [Header("Audio-Visual Elements")]
    [SerializeField] CanvasGroup faceDown;
    [SerializeField] CanvasGroup faceUp;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text attackText;
    [SerializeField] TMP_Text defenseText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] Image frameImage;
    [SerializeField] Image frontFace;
    [SerializeField] Image backFace;
    bool rotateCard = false;
    bool mouseIsHovering = false;
    Color startColor;

    public void SetupValues(CardData data)
    {
        cardData = data;

        //Visuals
        GetComponent<RectTransform>().sizeDelta = cardData.dimensions;
        frontFace.GetComponent<RectTransform>().sizeDelta = cardData.dimensions;
        backFace.GetComponent<RectTransform>().sizeDelta = cardData.dimensions;
        frameImage.GetComponent<RectTransform>().sizeDelta = cardData.dimensions;
        nameText.text = cardData.cardName;
        frontFace.sprite = cardData.frontFace;
        backFace.sprite = cardData.backFace;
        frameImage.sprite = cardData.pictureFrame;
        startColor = frontFace.color;

        //Text
        attackText.enabled = cardData.showAttack;
        defenseText.enabled = cardData.showDefense;
        descriptionText.enabled = cardData.showDescription;
        attackText.GetComponent<RectTransform>().anchoredPosition = cardData.attackPosition;
        defenseText.GetComponent<RectTransform>().anchoredPosition = cardData.defensePosition;
        descriptionText.GetComponent<RectTransform>().anchoredPosition = cardData.descriptionPosition;
        attackText.text = cardData.attack.ToString();
        defenseText.text = cardData.defense.ToString();
        descriptionText.text = cardData.descriptionText;
        Attack = cardData.attack;
        Defense = cardData.defense;

        //Set State
        faceDown.alpha = 1;
        faceUp.alpha = 0;
        SetState(CardState.FaceDown);
    }

    public void SetState(CardState state)
    {
        CardState = state;

        switch (CardState)
        {
            case CardState.FaceDown:
                rotateCard = false;
                break;
            case CardState.FaceUp:
                rotateCard = true;
                break;
            case CardState.Placed:
                rotateCard = false;
                faceUp.blocksRaycasts = false;
                break;
        }
    }

    #region Pointers
    public void OnPointerDown(PointerEventData eventData)
    {
        //
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MainManager.HandCards.ClickOnCard(gameObject.GetComponent<RectTransform>());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseIsHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseIsHovering = false;
    }

    #endregion

    #region Turn Card
    public async void TurnCard(float speedOfTurn)
    {
        if(speedOfTurn == 0f)
        {
            faceDown.alpha = 0;
            faceUp.alpha = 1;
            SetState(CardState.FaceUp);
            return;
        }

        MainManager.Audio.PlaySound(cardEffects.clipTurn, 0.4f, Random.Range(0.95f, 1.05f));

        RectTransform myTransform = GetComponent<RectTransform>();
        Quaternion startRot = myTransform.localRotation;
        Quaternion turnRot = Quaternion.identity;
        Vector3 sR = myTransform.localRotation.eulerAngles;
        float yRot = 0;
        float remainingTime = 0;
        float halfTurn = speedOfTurn / 2f;

        while (remainingTime < halfTurn)
        {
            remainingTime += Time.deltaTime;
            yRot = (remainingTime / halfTurn) * -90;

            myTransform.rotation = Quaternion.Euler(sR.x, yRot, sR.z);

            await Awaitable.NextFrameAsync();
        }

        faceDown.alpha = 0;
        faceUp.alpha = 1;

        remainingTime = 0;

        while (remainingTime < halfTurn)
        {
            remainingTime += Time.deltaTime;
            yRot = -90 + (remainingTime / halfTurn) * 90;
            myTransform.rotation = Quaternion.Euler(sR.x, yRot, sR.z);

            await Awaitable.NextFrameAsync();
        }

        myTransform.rotation = startRot;
        SetState(CardState.FaceUp);
    }

    #endregion

    #region Rotate And Hover

    private void Update()
    {
        LookAtMouse();
    }

    private void LookAtMouse()
    {
        if(!rotateCard)
            return;

        float mouseTiltX = 0f;
        float mouseTiltY = 0f;

        if (mouseIsHovering)
        {
            Vector2 imgPos = transform.position;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 finalPos = imgPos - mousePos;

            mouseTiltX = finalPos.y * cardEffects.mouseRotationMulti.x;
            mouseTiltY = finalPos.x * cardEffects.mouseRotationMulti.y;
        }

        float sine = Mathf.Sin(Time.time);
        float cosine = Mathf.Cos(Time.time);
        float lerpX = Mathf.LerpAngle(transform.eulerAngles.x, mouseTiltX + (sine * cardEffects.autoRotationMulti.x), cardEffects.rotationSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(transform.eulerAngles.y, mouseTiltY + (cosine * cardEffects.autoRotationMulti.y), cardEffects.rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(lerpX, lerpY, transform.eulerAngles.z);
    }

    #endregion

    #region Attack And Damage

    public void Damage(int dmg)
    {
        if(dmg > 0)
        {
            Defense -= dmg;

            Defense = Mathf.Clamp(Defense, 0, 99);
            defenseText.text = Defense.ToString();
            defenseText.rectTransform.DOPunchScale(cardEffects.damagedTextScale, 0.25f);
            frontFace.DOColor(Color.red, 0.25f).SetLoops(2, LoopType.Yoyo).OnComplete(ResetColor);
        }

        if(Defense == 0)
            AnimateDeath();
    }

    public void Heal(int amount)
    {
        Defense += amount;

        Defense = Mathf.Clamp(Defense, 0, 99);
        defenseText.text = Defense.ToString();
        defenseText.rectTransform.DOPunchScale(cardEffects.damagedTextScale, 0.25f);
        frontFace.DOColor(Color.green, 0.25f).SetLoops(2, LoopType.Yoyo).OnComplete(ResetColor);
    }

    private void ResetColor()
    {
        frontFace.color = startColor;
    }

    public void AnimateAttack(float duration, Vector2 attackTowards, float moveMultiplier)
    {
        MainManager.Audio.PlaySound(cardEffects.clipAttack, 0.5f, Random.Range(0.95f, 1.05f));
        Vector2 attackDirection = attackTowards - GetComponent<RectTransform>().anchoredPosition;
        attackDirection.Normalize();
        attackDirection *= cardEffects.attackDistance * moveMultiplier;
        Vector2 attackPos = GetComponent<RectTransform>().anchoredPosition + attackDirection;
        GetComponent<RectTransform>().DOAnchorPos(attackPos, duration/2).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
    }

    public async void AnimateDeath()
    {
        if (MySlot)
            MySlot.myCard = null;

        await Awaitable.WaitForSecondsAsync(cardEffects.dieDuration / 2);

        MainManager.Audio.PlaySound(cardEffects.clipDie, 0.4f, Random.Range(0.95f, 1.05f));
        GetComponent<RectTransform>().DOScale(0.1f, cardEffects.dieDuration / 2);
        GetComponent<RectTransform>().DOShakeRotation(cardEffects.dieDuration / 2, 90);

        await Awaitable.WaitForSecondsAsync(cardEffects.dieDuration / 2);

        Destroy(gameObject);
    }

    #endregion
}

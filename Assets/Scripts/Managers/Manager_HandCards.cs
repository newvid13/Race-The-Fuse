using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

[Serializable]
public class HandCardPool
{
    public CardData[] startingCards;
    public CardData[] cardsPool;

    public int totalWeight;

    public void SetRandomData()
    {
        foreach (CardData card in cardsPool)
            totalWeight += card.pickWeight;
    }
}

public class Manager_HandCards : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }
    public bool IsActive { get; set; }

    [Header("Hand")]
    public List<RectTransform> handCards = new List<RectTransform>();
    public int maxNumberCards;
    public int maxPullPerTurn;
    public RectTransform SelectedCard { get; set; }
    [SerializeField] HandShape_Preset handShape;
    float handSize;

    [Header("Spawning")]
    [SerializeField] RectTransform theHand;
    [SerializeField] Vector2 spawnPosition;
    [SerializeField] Vector3 spawnRotation;
    [SerializeField] Vector2 turnPosition;
    [SerializeField] float moveToTurnTime;
    [SerializeField] float turnTime;
    [SerializeField] float handMoveTime;
    [SerializeField] GameObject playerCardVisual;
    public HandCardPool levelCardPool;

    [Header("Select")]
    [SerializeField] AudioClip clipSelect;
    [SerializeField] Vector3 scaleOnSelected;
    [SerializeField] float positionOnSelected;

    public void SetupValues()
    {
        Status = ManagerStatus.Starting;
        IsActive = false;
        levelCardPool.SetRandomData();
        Status = ManagerStatus.Activated;
    }

    #region Start

    public async Task StartingPull()
    {
        if (levelCardPool.startingCards.Length > 0)
             await PullCards(levelCardPool.startingCards);
        else
            await PullCards(4);
    }

    #endregion

    #region Pull Cards

    public async Task PullCards(int numberOfCards)
    {
        IsActive = false;

        MainManager.Cameras.SwitchCamera(CameraType.Main, handMoveTime);
        await Awaitable.WaitForSecondsAsync(handMoveTime);

        for (int i = 0; i < numberOfCards; i++)
        {
            bool lastCard = false;
            if((i+1) == numberOfCards)
                lastCard = true;

            CardData cD = PickRandomCard(lastCard);
            await PullCard(cD);
        }

        await Awaitable.WaitForSecondsAsync(handMoveTime);
        IsActive = true;
    }

    private CardData PickRandomCard(bool lastCard)
    {
        if (lastCard && MainManager.Turn.Fuses.Count == 1)
        {
            int lastRow = MainManager.TableCards.laneHeight - 1;
            bool playerCardsInLastRow = false;

            for (int x = 0; x < MainManager.TableCards.width; x++)
            {
                if (MainManager.TableCards.Grid[x, lastRow].myCard?.GetComponent<Card_Player>() != null)
                {
                    playerCardsInLastRow = true;
                }
            }

            if (!playerCardsInLastRow && !handCards.Exists(item => item.GetComponent<Card_Base>().cardData == MainManager.Turn.FuseData))
            {
                float fuseChance = Random.Range(0f, 1f);
                if (fuseChance < 0.8f)
                    return MainManager.Turn.FuseData;
            }
        }

        int rng = Random.Range(1, levelCardPool.totalWeight + 1);
        int addedWeight = 0;

        for(int i = 0; i < levelCardPool.cardsPool.Length; i++)
        {
            if(rng <= levelCardPool.cardsPool[i].pickWeight + addedWeight)
                return levelCardPool.cardsPool[i];

            addedWeight += levelCardPool.cardsPool[i].pickWeight;
        }

        return null;
    }

    public async Task PullCards(CardData[] set)
    {
        IsActive = false;

        MainManager.Cameras.SwitchCamera(CameraType.Main, handMoveTime);
        await Awaitable.WaitForSecondsAsync(handMoveTime);

        for (int i = 0; i < set.Length; i++)
            await PullCard(set[i]);

        await Awaitable.WaitForSecondsAsync(handMoveTime);
        IsActive = true;
    }

    private async Task PullCard(CardData id)
    {
        //Instantiate, set position and rotation
        RectTransform temp = Instantiate(playerCardVisual, theHand).GetComponent<RectTransform>();
        temp.GetComponent<Card_Base>().SetupValues(id);
        temp.anchoredPosition = spawnPosition;
        temp.rotation = Quaternion.Euler(spawnRotation);

        //Add to list
        handCards.Add(temp.GetComponent<RectTransform>());
        handSize += temp.sizeDelta.x + handShape.cardDistance;

        //Move, turn, add to hand
        temp.DOAnchorPos3D(turnPosition, moveToTurnTime);
        temp.DORotate(Vector3.zero, moveToTurnTime);

        await Awaitable.WaitForSecondsAsync(moveToTurnTime);
        temp.GetComponent<Card_Base>().TurnCard(turnTime);

        await Awaitable.WaitForSecondsAsync(turnTime + 0.2f);
        OrganizeHand(fastSort:false);
    }

    #endregion

    #region Organize Hand
    private void OrganizeHand(bool fastSort)
    {
        if (handCards.Count == 0)
            return;

        for (int i = 0; i < handCards.Count; i++)
        {
            SetPositionAndRotation(i, fastSort);
        }
    }

    private void SetPositionAndRotation(int cardIndex, bool instantChange)
    {
        float cardWidth = handCards[cardIndex].sizeDelta.x;
        float allCardWidth = 0f;
        for (int i = 0; i < cardIndex; i++)
            allCardWidth += handCards[i].sizeDelta.x + handShape.cardDistance;

        float xOffset = -handSize / 2 + allCardWidth + cardWidth / 2;
        float yOffset = handShape.handPositionCurve.Evaluate(((float)cardIndex + 0.5f) / (float)handCards.Count) * handShape.handHeightOffset;
        float rotOffset = handShape.handRotationCurve.Evaluate(((float)cardIndex + 0.5f) / (float)handCards.Count) * handShape.handRotationOffset;

        Vector3 cardRot = new Vector3(handCards[cardIndex].rotation.x, handCards[cardIndex].rotation.y, rotOffset);
        Vector2 cardPos = new Vector2(xOffset, yOffset);

        if(instantChange)
        {
            handCards[cardIndex].anchoredPosition = cardPos;
            handCards[cardIndex].rotation = Quaternion.Euler(cardRot);
        }
        else
        {
            handCards[cardIndex].DOAnchorPos3D(cardPos, handMoveTime);
            handCards[cardIndex].DORotate(cardRot, handMoveTime);
        }
    }

    public void ToggleHand(bool toggle, float fadeSpeed)
    {
        CanvasGroup groupHand = theHand.GetComponent<CanvasGroup>();

        if (toggle)
            groupHand.DOFade(1f, fadeSpeed);
        else
            groupHand.DOFade(0f, fadeSpeed);
    }

    #endregion

    #region Clicks
    public void ClickOnCard(RectTransform sentCard)
    {
        if(MainManager.Turn.IsRunningTurn || !IsActive)
            return;

        if (sentCard == SelectedCard)
        {
            DeselectCard();
        }
        else
        {
            if (SelectedCard != null)
                DeselectCard();

            SelectCard(sentCard);
        }
    }

    private void SelectCard(RectTransform cardToSelect)
    {
        MainManager.Audio.PlaySound(clipSelect, 0.4f);

        Vector2 locPos = cardToSelect.localPosition;
        locPos.y += positionOnSelected;
        cardToSelect.localPosition = locPos;
        cardToSelect.localScale = scaleOnSelected;
        cardToSelect.DOShakeRotation(0.2f, 20f);

        SelectedCard = cardToSelect;
        SelectedCard.GetComponent<Card_Base>().cardData.action.CardSelected();
    }

    private void DeselectCard()
    {
        if (SelectedCard == null)
            return;

        MainManager.Audio.PlaySound(clipSelect, 0.4f);

        SelectedCard.localScale = new Vector3(1f, 1f, 1f);
        SetPositionAndRotation(handCards.IndexOf(SelectedCard), true);
        SelectedCard.GetComponent<Card_Base>().cardData.action.CardDeselected();
        SelectedCard = null;
    }

    public void RemoveCardFromHand(RectTransform card)
    {
        card.localScale = new Vector3(1f, 1f, 1f);
        handCards.Remove(card);
        handSize -= (card.sizeDelta.x + handShape.cardDistance);
        SelectedCard = null;
        OrganizeHand(fastSort: true);
    }

    #endregion
}

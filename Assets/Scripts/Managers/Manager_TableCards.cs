using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public delegate void SlotClicked(int x, int y);
public delegate void SlotHover(int x, int y, bool state);

[Serializable]
public class SlotCardPool
{
    public CardData[] laneLeft;
    public CardData[] laneMid;
    public CardData[] laneRight;

    public CardData[] enemyCardsPool;
}

public class Manager_TableCards : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }
    public Slot_Base[,] Grid { get; set; }
    public Slot_Base SlotBoss { get; set; }
    public int laneHeight;
    [HideInInspector] public int width;

    [Header("Spawning")]
    [SerializeField] float spawnSpeed;
    [SerializeField] RectTransform[] laneParents;
    [SerializeField] RectTransform gridParent;
    [SerializeField] Vector2 slotDistance;
    [SerializeField] Vector2 bossDistance;
    [SerializeField] GameObject startSlotPrefab;
    [SerializeField] GameObject standardSlotPrefab;
    [SerializeField] GameObject slotConnectorPrefab;

    [Header("Enemy Cards")]
    [SerializeField] GameObject enemyCardVisual;
    [SerializeField] CardData enemyBossPrefab;
    [SerializeField] SlotCardPool levelEnemyPool;

    [Header("Effects")]
    [SerializeField] Image[] effects;

    #region Start
    public void SetupValues()
    {
        Status = ManagerStatus.Starting;

        width = laneParents.Length;
        Grid = new Slot_Base[width, laneHeight];

        for (int x = 0; x < width; x++)
            CreateLane(x);

        CreateBossSlot();

        Status = ManagerStatus.Activated;
    }

    public async Task StartingFillSlots()
    {
        MainManager.Cameras.SwitchCamera(CameraType.Slots, spawnSpeed);
        await Awaitable.WaitForSecondsAsync(spawnSpeed);

        Dictionary<int, CardData[]> lanes = new Dictionary<int, CardData[]>();
        lanes.Add(0, levelEnemyPool.laneLeft);
        lanes.Add(1, levelEnemyPool.laneMid);
        lanes.Add(2, levelEnemyPool.laneRight);

        for (int x = 0; x < width; x++)
        {
            CardData[] currentLane = lanes[x];
            if (currentLane.Length > laneHeight - 1 || currentLane.Length == 0)
                await FillRandomSlots(x);
            else
                await FillSetSlots(x, currentLane);
        }

        await Awaitable.WaitForSecondsAsync(spawnSpeed * 5);
        CreateCardInSlot(enemyBossPrefab, 0, laneHeight + 1, 0.5f);
        await Awaitable.WaitForSecondsAsync(spawnSpeed*10);

        async Task FillSetSlots(int laneX, CardData[] currentLane)
        {
            for (int y = 0; y < currentLane.Length; y++)
            {
                if (currentLane[y] == null)
                    continue;

                CreateCardInSlot(currentLane[y], laneX, y + 1, 0.3f);

                await Awaitable.WaitForSecondsAsync(spawnSpeed);
            }
        }

        async Task FillRandomSlots(int laneX)
        {
            for (int y = 1; y < laneHeight; y++)
            {
                CardData randomCard = levelEnemyPool.enemyCardsPool[Random.Range(0, levelEnemyPool.enemyCardsPool.Length)];
                CreateCardInSlot(randomCard, laneX, y, 0.3f);

                await Awaitable.WaitForSecondsAsync(spawnSpeed);
            }
        }
    }

    private void CreateCardInSlot(CardData cD, int x, int y, float turnSpeed)
    {
        RectTransform temp = Instantiate(enemyCardVisual).GetComponent<RectTransform>();
        temp.GetComponent<Card_Base>().SetupValues(cD);
        PlaceCardInSlot(temp, x, y);
        temp.GetComponent<Card_Base>().TurnCard(turnSpeed);
    }

    #endregion

    #region Spawn Slots

    private void CreateLane(int laneNumber)
    {
        Vector2 slotImageSize = standardSlotPrefab.GetComponent<RectTransform>().sizeDelta;
        Vector3 lanePos = laneParents[laneNumber].anchoredPosition;
        Quaternion laneRot = laneParents[laneNumber].rotation;
        float connectorOffset = slotImageSize.y / 2 + slotDistance.y / 2;

        for (int y = 0; y < laneHeight; y++)
        {
            //Slot Position
            Vector2 nodePos = lanePos + laneParents[laneNumber].up * (slotImageSize.y * y + (y * slotDistance.y));
            GameObject temp = Instantiate(y == 0 ? startSlotPrefab : standardSlotPrefab, gridParent);
            temp.GetComponent<Slot_Base>().SetupValues(laneNumber, y, nodePos, laneRot);
            Grid[laneNumber, y] = temp.GetComponent<Slot_Base>();


            //Connector Position
            if (y == laneHeight - 1)
            {
                laneRot.z *= 4f;
                connectorOffset = slotImageSize.y / 2 + bossDistance.y;
            }
            Vector2 connectorPos = lanePos + laneParents[laneNumber].up * (slotImageSize.y * y + (y * slotDistance.y) + connectorOffset);
            GameObject tempCon = Instantiate(slotConnectorPrefab, connectorPos, laneRot, gridParent);
            tempCon.GetComponent<RectTransform>().anchoredPosition = connectorPos;
        }
    }

    private void CreateBossSlot()
    {
        Vector2 slotImageSize = standardSlotPrefab.GetComponent<RectTransform>().sizeDelta;
        Vector3 lanePos = laneParents[1].anchoredPosition;
        Quaternion laneRot = laneParents[1].rotation;

        Vector2 nodePos = lanePos + laneParents[1].up * (slotImageSize.y * laneHeight + laneHeight * slotDistance.y + bossDistance.y);
        GameObject temp = Instantiate(standardSlotPrefab, gridParent);
        temp.GetComponent<Slot_Base>().SetupValues(0, laneHeight + 1, nodePos, laneRot);
        SlotBoss = temp.GetComponent<Slot_Base>();
    }

    #endregion

    #region Highlight Slots

    public void HighlightSlots(bool toggle)
    {
        if (!toggle)
        {
            foreach (Slot_Base slot in Grid)
            {
                slot.ToggleHighlight(false);
                SlotBoss.ToggleHighlight(false);
            }
        }
    }
    #endregion

    #region Card Movement

    public void PlaceCardInSlot(RectTransform card, int x, int y)
    {
        card.GetComponent<Card_Base>().SetState(CardState.Placed);
        card.SetParent(gridParent, false);

        if (y > laneHeight)
        {
            card.anchoredPosition = SlotBoss.position;
            card.rotation = SlotBoss.rot;

            SlotBoss.myCard = card.gameObject;
            card.GetComponent<Card_Base>().MySlot = SlotBoss;
        }
        else
        {
            card.anchoredPosition = Grid[x, y].position;
            card.rotation = Grid[x, y].rot;

            Grid[x, y].myCard = card.gameObject;
            card.GetComponent<Card_Base>().MySlot = Grid[x, y];
        }
    }

    public async void MoveCardFromTo(RectTransform card, int startX, int startY, int endX, int endY)
    {
        if (Grid[endX, endY].myCard == null)
        {
            MainManager.Audio.PlaySound(card.GetComponent<Card_Base>().cardEffects.clipMove, 0.3f);

            card.anchoredPosition = Grid[endX, endY].position;
            card.rotation = Grid[endX, endY].rot;

            Grid[endX, endY].myCard = card.gameObject;
            card.GetComponent<Card_Base>().MySlot = Grid[endX, endY];

            Grid[startX, startY].myCard = null;
        }
        else
        {
            Grid[startX, startY].myCard.GetComponent<Card_Base>().AnimateAttack(0.2f, Grid[endX, endY].position, 2f);

            await Awaitable.WaitForSecondsAsync(0.2f);

            MainManager.Audio.PlaySound(Grid[startX, startY].myCard.GetComponent<Card_Base>().cardEffects.clipHurt);
            Grid[startX, startY].myCard.GetComponent<Card_Base>().Damage(1);
        }
    }

    #endregion

    #region Effect Images

    public void SetEffectImages(Sprite effectSprite, int imgOrder, Vector2 slotPosition, Vector2 effectOffset)
    {
        if(imgOrder >= effects.Length)
        {
            SetEffectImages(false);
            return;
        }

        effects[imgOrder].sprite = effectSprite;
        effects[imgOrder].rectTransform.anchoredPosition = slotPosition + effectOffset;
        effects[imgOrder].gameObject.SetActive(true);
    }

    public void SetEffectImages(Sprite[] effectSprites, Vector2[] effectPositions, Vector2 slotPosition)
    {
        if(effectSprites.Length > effects.Length)
        {
            SetEffectImages(false);
            return;
        }

        for(int i = 0; i < effectSprites.Length; i++)
        {
            effects[i].sprite = effectSprites[i];
            effects[i].rectTransform.anchoredPosition = slotPosition + effectPositions[i];
            effects[i].gameObject.SetActive(true);
        }
    }

    public void SetEffectImages(bool state)
    {
        foreach(Image effect in effects)
            effect.gameObject.SetActive(false);
    }

    #endregion
}

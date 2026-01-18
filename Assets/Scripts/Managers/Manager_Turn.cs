using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

public class Manager_Turn : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }
    public bool IsRunningTurn { get; private set; }

    [Header("Fuses Spawning")]
    public Stack<GameObject> Fuses { get; private set; }
    public CardData FuseData;

    [SerializeField] RectTransform fuseParent;
    [SerializeField] float fuseDistance;
    [SerializeField] GameObject playerCardVisual;
    [SerializeField] int fuseStartNumber;
    [SerializeField] GameObject fuseSparks;

    [Header ("Timers")]
    [SerializeField] float sequencesPause;
    [SerializeField] float battleDuration;
    [SerializeField] float cardMovementDuration;
    [SerializeField] float fuseRemoveDuration;
    [SerializeField] float fuseAddDuration;

    [Header("End Game")]
    [SerializeField] GameObject endLight;
    [SerializeField] float endDuration;

    [Header("Audio")]
    [SerializeField] AudioClip clipExplosion;
    [SerializeField] AudioClip clipVictory;
    [SerializeField] AudioClip clipStartGame;
    [SerializeField] AudioClip clipStartTurn;

    #region Start

    public void SetupValues()
    {
        Status = ManagerStatus.Starting;

        Fuses = new Stack<GameObject>();
        Vector2 top = MainManager.TableCards.SlotBoss.position;
        top.y += 265;
        fuseParent.anchoredPosition = top;
        endLight.transform.position = fuseParent.transform.position;

        Status = ManagerStatus.Activated;
    }

    public async Task StartingFusePull()
    {
        MainManager.Cameras.SwitchCamera(CameraType.Fuses, sequencesPause);
        await Awaitable.WaitForSecondsAsync(sequencesPause);

        MainManager.Audio.PlaySound(clipStartGame, 0.75f);

        for (int i = 0; i < fuseStartNumber; i++)
        {
            RectTransform temp = Instantiate(playerCardVisual, fuseParent).GetComponent<RectTransform>();
            temp.GetComponent<Card_Base>().SetupValues(FuseData);
            temp.GetComponent<Card_Base>().TurnCard(0);
            AddFuse(temp);
            await Awaitable.WaitForSecondsAsync(0.1f);
        }

        fuseSparks.SetActive(true);
        await Awaitable.WaitForSecondsAsync(fuseAddDuration*3);

        MainManager.UI.UpdateTurnsText(Fuses.Count.ToString());
    }

    #endregion

    #region Turn

    public async void PlayTurn()
    {
        if (IsRunningTurn || !MainManager.HandCards.IsActive)
            return;

        IsRunningTurn = true;
        MainManager.UI.ToggleTurn(true);
        MainManager.Cameras.SwitchCamera(CameraType.Slots, sequencesPause);
        MainManager.Audio.PlaySound(clipStartTurn, 0.4f);

        //await Awaitable.WaitForSecondsAsync(0.3f);
        await Awaitable.WaitForSecondsAsync(sequencesPause);

        await TurnBattle();
        await TurnMove();

        if (MainManager.TableCards.SlotBoss.myCard == null)
        {
            TurnGameWon();
            return;
        }

        await TurnFuse();

        if (Fuses.Count > 0)
        {
            await TurnPullCards();
        }
        else
        {
            TurnGameLost();
            return;
        }

        TurnEnd();
    }

    private async Task TurnBattle()
    {
        MainManager.UI.UpdateSequenceText("BATTLE");

        int height = MainManager.TableCards.laneHeight;
        int width = MainManager.TableCards.width;

        //MainManager.Cameras.SwitchCamera(CameraType.Slots, sequencesPause);
        await Awaitable.WaitForSecondsAsync(sequencesPause);

        await Awaitable.WaitForSecondsAsync(battleDuration);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Slot_Base slotDown = MainManager.TableCards.Grid[x, y];
                Slot_Base slotUp = ReturnUpperSlot(x, y + 1);

                if (slotDown.myCard == null || slotUp.myCard == null)
                    continue;

                Card_Player cP = slotDown.myCard.GetComponent<Card_Player>();
                Card_Enemy cE = slotUp.myCard.GetComponent<Card_Enemy>();

                if (cP != null && cE != null)
                {
                    await Battle(slotDown, slotUp, cP, cE);
                }
            }
        }

        await Awaitable.WaitForSecondsAsync(sequencesPause);

        async Task Battle(Slot_Base playerSlot, Slot_Base enemySlot, Card_Player cP, Card_Enemy cE)
        {
            if(cP.Attack > 0)
                cP.AnimateAttack(battleDuration, enemySlot.position, 1f);

            if(cE.Attack > 0)
                cE.AnimateAttack(battleDuration, playerSlot.position, 1f);

            cP.Damage(cE.Attack);
            cE.Damage(cP.Attack);
            await Awaitable.WaitForSecondsAsync(battleDuration*2);
        }

        Slot_Base ReturnUpperSlot(int x, int yUpper)
        {
            if(yUpper < height)
                return (MainManager.TableCards.Grid[x, yUpper]);
            else
                return (MainManager.TableCards.SlotBoss);
        }
    }

    private async Task TurnMove()
    {
        MainManager.UI.UpdateSequenceText("MOVEMENT");

        int height = MainManager.TableCards.laneHeight;
        int width = MainManager.TableCards.width;

        for (int y = height - 2; y > -1; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Slot_Base slotDown = MainManager.TableCards.Grid[x, y];
                Slot_Base slotUp = MainManager.TableCards.Grid[x, y + 1];

                if (slotDown.myCard == null || slotUp.myCard != null)
                    continue;

                Card_Player cP = slotDown.myCard.GetComponent<Card_Player>();
                if (cP != null)
                {
                    MainManager.TableCards.MoveCardFromTo(cP.GetComponent<RectTransform>(), x, y, x, y + 1);
                    await Awaitable.WaitForSecondsAsync(cardMovementDuration);
                }
            }
        }

        await Awaitable.WaitForSecondsAsync(sequencesPause);
    }

    private async Task TurnFuse()
    {
        MainManager.UI.UpdateSequenceText("FUSE");

        MainManager.Cameras.UpdateFrame(CameraType.Fuses);
        MainManager.Cameras.UpdateFrame(CameraType.Main);
        MainManager.Cameras.SwitchCamera(CameraType.Fuses, sequencesPause);
        await Awaitable.WaitForSecondsAsync(sequencesPause);

        await Awaitable.WaitForSecondsAsync(fuseRemoveDuration);

        RemoveFuse();

        await Awaitable.WaitForSecondsAsync(fuseRemoveDuration*2);

        await Awaitable.WaitForSecondsAsync(sequencesPause);
    }
    
    private async Task TurnPullCards()
    {
        MainManager.UI.ToggleTurn(false);
        MainManager.Cameras.SwitchCamera(CameraType.Main, sequencesPause);
        await Awaitable.WaitForSecondsAsync(sequencesPause);

        int numOfCards = MainManager.HandCards.handCards.Count;
        int maxCards = MainManager.HandCards.maxNumberCards;
        int numToPull = maxCards - numOfCards;
        numToPull = Mathf.Clamp(numToPull, 0, MainManager.HandCards.maxPullPerTurn);
        MainManager.HandCards.PullCards(numToPull);

        await Awaitable.WaitForSecondsAsync(sequencesPause);
    }

    private void TurnEnd()
    {
        IsRunningTurn = false;
    }

    private async void TurnGameLost()
    {
        MainManager.Audio.StopAllMusic();
        SetSparkPosition();
        await Awaitable.WaitForSecondsAsync(endDuration);

        endLight.SetActive(true);
        fuseSparks.SetActive(false);
        DOVirtual.Float(0, 2, 0.5f, value => endLight.GetComponent<Light2D>().intensity = value);
        MainManager.Audio.PlaySound(clipExplosion, 0.5f);

        await Awaitable.WaitForSecondsAsync(endDuration);

        MainManager.UI.ToggleEnding(victory: false);
    }

    private void TurnGameWon()
    {
        MainManager.Audio.StopAllMusic();
        MainManager.Audio.PlaySound(clipVictory);

        MainManager.UI.ToggleEnding(victory: true);
    }

    #endregion

    #region Fuse

    public void AddFuse(RectTransform newFuse)
    {
        newFuse.GetComponent<Card_Base>().SetState(CardState.Placed);
        newFuse.SetParent(fuseParent, false);

        Vector2 fuseImageSize = newFuse.sizeDelta;
        newFuse.anchoredPosition = new Vector2(0, fuseImageSize.y * Fuses.Count + fuseDistance * Fuses.Count);
        newFuse.rotation = Quaternion.identity;

        Fuses.Push(newFuse.gameObject);
        SetSparkPosition();
        MainManager.UI.UpdateTurnsText(Fuses.Count.ToString());
    }

    public async void RemoveFuse()
    {
        GameObject lastFuse = Fuses.Pop();
        lastFuse.GetComponent<Card_Base>().AnimateDeath();

        await Awaitable.WaitForSecondsAsync(1f);
        SetSparkPosition();
        MainManager.UI.UpdateTurnsText(Fuses.Count.ToString());
    }

    private void SetSparkPosition()
    {
        Vector3 sparkPos = Vector3.zero;

        if (Fuses.Count == 0)
        {
            sparkPos = fuseParent.transform.position;
            sparkPos.y -= 1f;
            sparkPos.z = -1;
        }
        else
        {
            sparkPos = Fuses.Peek().transform.position;
            sparkPos.y += 1.8f;
            sparkPos.z = -1;
        }

        fuseSparks.transform.position = sparkPos;
    }

    #endregion
}

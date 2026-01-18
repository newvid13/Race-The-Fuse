using System;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public static Manager_HandCards HandCards;
    public static Manager_TableCards TableCards;
    public static Manager_Turn Turn;
    public static Manager_Cameras Cameras;
    public static Manager_Audio Audio;
    public static Manager_UI UI;

    List<IGameManager> startSequence = new List<IGameManager>();
    public Action gameStarted;

    private void Awake()
    {
        Instance = this;
        HandCards = GetComponent<Manager_HandCards>();
        TableCards = GetComponent<Manager_TableCards>();
        Turn = GetComponent<Manager_Turn>();
        Cameras = GetComponent<Manager_Cameras>();
        Audio = GetComponent<Manager_Audio>();
        UI = GetComponent<Manager_UI>();

        if (TableCards) startSequence.Add(TableCards);
        if (Turn) startSequence.Add(Turn);
        if (HandCards) startSequence.Add(HandCards);
        if (Cameras) startSequence.Add(Cameras);
        if (Audio) startSequence.Add(Audio);
        if (UI) startSequence.Add(UI);

        StartupManagers();
    }

    private async void StartupManagers()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        int num = 0;
        while (num < startSequence.Count)
        {
            if (startSequence[num].Status == ManagerStatus.Disabled)
            {
                startSequence[num].SetupValues();
                Debug.Log("Starting " + startSequence[num].GetType());
            }

            if (startSequence[num].Status == ManagerStatus.Activated)
                num++;
            else
                await Awaitable.NextFrameAsync();
        }

        Debug.Log("All Managers Started Up, Starting Game");

        StartupGame();
    }

    private async void StartupGame()
    {
        await MainManager.UI.StartingFade();
        await MainManager.TableCards.StartingFillSlots();
        await MainManager.Turn.StartingFusePull();
        await MainManager.HandCards.StartingPull();

        MainManager.UI.ToggleTurn(false);
        MainManager.Audio.StartingMusicPlay();

        gameStarted?.Invoke();
    }
}

public enum CardState
{
    FaceDown,
    FaceUp,
    Placed
}

public enum CameraType
{
    Main,
    Slots,
    Fuses
}

public enum ManagerStatus
{
    Disabled,
    Starting,
    Activated
}

public interface IGameManager
{
    public ManagerStatus Status { get; }

    public void SetupValues();
}
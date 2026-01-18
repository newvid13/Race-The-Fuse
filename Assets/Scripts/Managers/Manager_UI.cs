using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;

public class Manager_UI : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }

    [SerializeField] CanvasGroup blackPanel;
    [SerializeField] CanvasGroup mainPanel;
    [SerializeField] CanvasGroup turnPanel;
    [SerializeField] TMP_Text turnSequenceText;
    [SerializeField] TMP_Text turnsUntilEndText;
    [SerializeField] float UIFadeSpeed;
    [SerializeField] GameObject panelVictory;
    [SerializeField] GameObject panelDefeat;

    public void SetupValues()
    {
        Status = ManagerStatus.Starting;

        mainPanel.alpha = 0;
        turnPanel.alpha = 0;
        mainPanel.blocksRaycasts = false;

        Status = ManagerStatus.Activated;
    }

    public async Task StartingFade()
    {
        await Awaitable.WaitForSecondsAsync(0.1f);
        blackPanel.DOFade(0, 0.1f);
        await Awaitable.WaitForSecondsAsync(0.1f);
    }

    public void Replay()
    {
        MainManager.Audio.StopAllMusic();
        DOTween.KillAll();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void LoadLevel(string id)
    {
        MainManager.Audio.StopAllMusic();
        DOTween.KillAll();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
    }

    public void ToggleTurn(bool state)
    {
        if(state)
        {
            mainPanel.DOFade(0, UIFadeSpeed);
            turnPanel.DOFade(1, UIFadeSpeed);
            mainPanel.blocksRaycasts = false;

            MainManager.HandCards.ToggleHand(false, UIFadeSpeed);
            MainManager.TableCards.HighlightSlots(false);
            if (MainManager.HandCards.SelectedCard != null)
                MainManager.HandCards.ClickOnCard(MainManager.HandCards.SelectedCard);
        }
        else
        {
            turnPanel.DOFade(0, UIFadeSpeed);
            mainPanel.DOFade(1, UIFadeSpeed);
            mainPanel.blocksRaycasts = true;

            MainManager.HandCards.ToggleHand(true, UIFadeSpeed);
        }
    }

    public void UpdateSequenceText(string text)
    {
        turnSequenceText.text = "SEQUENCE : " + text;
    }

    public void UpdateTurnsText(string text)
    {
        turnsUntilEndText.text = "TURNS UNTIL EXPLOSION : " + text;
    }

    public void ToggleEnding(bool victory)
    {
        mainPanel.alpha = 0;
        turnPanel.alpha = 0;
        mainPanel.blocksRaycasts = false;

        if (victory)
            panelVictory.SetActive(true);
        else
            panelDefeat.SetActive(true);
    }
}

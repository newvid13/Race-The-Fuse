using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class TutorialElement
{
    public CameraType tutorialLocation;
    [TextArea(5, 10)]
    public string tutorialText;

    public Vector2 tutorialPosition;
    public float tutorialDuration;
}

public class Manager_Tutorial : MonoBehaviour
{
    public TutorialElement[] tutElements;
    [SerializeField] CanvasGroup tutorialGroup;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] AudioClip tutMessageClip;
    [SerializeField] AudioClip tutEnd;

    private void Start()
    {
        MainManager.Instance.gameStarted += RunTutorial;
    }

    private async void RunTutorial()
    {
        Debug.Log("Running Tutorial");
        MainManager.HandCards.IsActive = false;

        //Start tutorial
        await Awaitable.WaitForSecondsAsync(1);
        MainManager.Audio.PlaySound(tutMessageClip, 0.75f, Random.Range(0.95f, 1.05f));

        for (int i = 0; i < tutElements.Length; i++)
        {
            if(i != 0 && tutElements[i-1].tutorialLocation != tutElements[i].tutorialLocation)
                MainManager.Audio.PlaySound(tutMessageClip, 0.75f, Random.Range(0.95f, 1.05f));

            tutorialGroup.GetComponent<RectTransform>().anchoredPosition = tutElements[i].tutorialPosition;
            tutorialText.text = tutElements[i].tutorialText;
            tutorialGroup.alpha = 1;
            MainManager.Cameras.SwitchCamera(tutElements[i].tutorialLocation, 0.5f);
            await Awaitable.WaitForSecondsAsync(tutElements[i].tutorialDuration);
        }

        MainManager.Audio.PlaySound(tutEnd, 0.75f);
        tutorialGroup.alpha = 0;

        //Start game
        await Awaitable.WaitForSecondsAsync(0.5f);
        MainManager.HandCards.IsActive = true;
        MainManager.Cameras.SwitchCamera(CameraType.Main, 0.5f);
    }
}

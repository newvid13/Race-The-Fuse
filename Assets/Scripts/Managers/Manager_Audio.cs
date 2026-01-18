using DG.Tweening;
using System.Threading;
using UnityEngine;

public class Manager_Audio : MonoBehaviour, IGameManager
{
    public ManagerStatus Status { get; private set; }

    [Header("Effects")]
    [SerializeField] AudioSource audSounds;

    [Header("Music")]
    [SerializeField] AudioClip[] musicClips;
    [SerializeField] AudioSource audMusicOne, audMusicTwo;
    int currentMusicClip;
    CancellationTokenSource tokenSource = new CancellationTokenSource();

    public void SetupValues()
    {
        Status = ManagerStatus.Starting;
        Status = ManagerStatus.Activated;
    }

    public void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
            return;

        audSounds.pitch = pitch;
        audSounds.PlayOneShot(clip, volume);
    }

    public void StartingMusicPlay()
    {
        PlayRandomMusic(tokenSource.Token);
    }

    private async void PlayRandomMusic(CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
        {
            Debug.Log("Exiting");
            return;
        }

        //pick track
        int pickedMusicClip = 0;

        if(musicClips.Length > 1)
        {
            do
            {
                pickedMusicClip = Random.Range(0, musicClips.Length);
            } while (currentMusicClip == pickedMusicClip);
        }

        currentMusicClip = pickedMusicClip;

        //fade in track
        if (audMusicOne.isPlaying)
        {
            audMusicOne.DOFade(0f, 3f).OnComplete(StopMusicOne);
            audMusicTwo.volume = 0;
            audMusicTwo.PlayOneShot(musicClips[currentMusicClip]);
            audMusicTwo.DOFade(1f, 3f);
        }
        else
        {
            audMusicTwo.DOFade(0f, 3f).OnComplete(StopMusicTwo);
            audMusicOne.volume = 0;
            audMusicOne.PlayOneShot(musicClips[currentMusicClip]);
            audMusicOne.DOFade(1f, 3f);
        }

        //wait duration
        await Awaitable.WaitForSecondsAsync(musicClips[currentMusicClip].length - 5f);
        PlayRandomMusic(ct);
    }

    public void StopAllMusic()
    {
        tokenSource.Cancel();
        audMusicOne.DOFade(0f, 0.5f).OnComplete(StopMusicOne);
        audMusicTwo.DOFade(0f, 0.5f).OnComplete(StopMusicTwo);
    }

    private void StopMusicOne()
    {
        if (audMusicOne == null)
            return;

        audMusicOne.Stop();
    }

    private void StopMusicTwo()
    {
        if (audMusicTwo == null)
            return;

        audMusicTwo.Stop();
    }
}

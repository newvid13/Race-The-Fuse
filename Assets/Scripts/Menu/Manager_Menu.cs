using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Manager_Menu : MonoBehaviour
{
    public static Manager_Menu instance;
    public static Manager_Audio Audio;

    [Header("Spawning")]
    [SerializeField] Transform fuseParent;
    [SerializeField] RectTransform fusePrefab;
    [SerializeField] float spawnSpeed;
    [SerializeField] int rows;

    [SerializeField] GameObject objTitle;
    [SerializeField] GameObject[] objButtons;
    [SerializeField] AudioClip clipStart;

    [Header("Mixer")]
    [SerializeField] AudioMixer mixer;
    [SerializeField] float musicDefaultVolume;
    [SerializeField] float sfxDefaultVolume;
    bool sfxIsMuted;
    bool musicIsMuted;
    [SerializeField] UI_Button buttonSfx;
    [SerializeField] UI_Button buttonMusic;

    void Start()
    {
        HideAll();
        SetSoundButtons();

        instance = this;
        Audio = GetComponent<Manager_Audio>();
        Audio.SetupValues();

        StartingSpawn();
    }

    #region Start

    private async void StartingSpawn()
    {
        await Awaitable.WaitForSecondsAsync(spawnSpeed);
        await SpawnFuses();

        objTitle.SetActive(true);
        //Audio.PlaySound(clipStart);

        await Awaitable.WaitForSecondsAsync(0.5f);

        for (int i = 0; i < objButtons.Length; i++)
        {
            objButtons[i].SetActive(true);
            await Awaitable.WaitForSecondsAsync(spawnSpeed);
        }

        await Awaitable.WaitForSecondsAsync(1f);
        Audio.StartingMusicPlay();
    }

    private void HideAll()
    {
        objTitle.SetActive(false);
        for (int i = 0; i < objButtons.Length; i++)
        {
            objButtons[i].SetActive(false);
        }
    }

    private async Task SpawnFuses()
    {
        Vector2 fuseSize = fusePrefab.sizeDelta;
        Vector3 startFusePos = new Vector2(0, 157);
        Quaternion fuseRot = fusePrefab.rotation;

        for (int y = 0; y < rows; y++)
        {
            startFusePos.y -= 157;

            for (int x = 0; x < y + 1; x++)
            {
                Vector2 currentFusePos = startFusePos + fusePrefab.up * (fuseSize.y * x);
                RectTransform temp = Instantiate(fusePrefab, currentFusePos, fuseRot, fuseParent);
                temp.anchoredPosition = currentFusePos;
            }

            await Awaitable.WaitForSecondsAsync(spawnSpeed);
        }

        for (int y = rows - 1; y >= -1; y--)
        {
            startFusePos.x += 157f;

            for (int x = 0; x < y + 2; x++)
            {
                Vector2 currentFusePos = startFusePos + fusePrefab.up * (fuseSize.y * x);
                RectTransform temp = Instantiate(fusePrefab, currentFusePos, fuseRot, fuseParent);
                temp.anchoredPosition = currentFusePos;
            }

            await Awaitable.WaitForSecondsAsync(spawnSpeed);
        }

        await Awaitable.WaitForSecondsAsync(spawnSpeed);
    }

    #endregion

    #region Level Loading

    public void StartLevel(string id)
    {
        Audio.StopAllMusic();
        DOTween.KillAll();
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(id);
    }

    #endregion

    #region Sound Options

    private void SetSoundButtons()
    {
        float sfxVol;
        float musicVol;
        mixer.GetFloat("SfxVol", out  sfxVol);
        mixer.GetFloat("MusicVol", out musicVol);
        sfxIsMuted = sfxVol < -79f ? true : false;
        musicIsMuted = musicVol < -79f ? true : false;

        buttonSfx.ChangeIcon(isOn: !sfxIsMuted);
        buttonMusic.ChangeIcon(isOn: !musicIsMuted);
    }

    public void ToggleMuteSfx()
    {
        if (sfxIsMuted)
        {
            sfxIsMuted = false;
            mixer.DOSetFloat("SfxVol", sfxDefaultVolume, 1f);
        }
        else
        {
            sfxIsMuted = true;
            mixer.DOSetFloat("SfxVol", -80, 1f);
        }

        buttonSfx.ChangeIcon(isOn: !sfxIsMuted);
    }

    public void ToggleMuteMusic()
    {
        if (musicIsMuted)
        {
            musicIsMuted = false;
            mixer.DOSetFloat("MusicVol", musicDefaultVolume, 1f);
        }
        else
        {
            musicIsMuted = true;
            mixer.DOSetFloat("MusicVol", -80, 1f);
        }

        buttonMusic.ChangeIcon(isOn: !musicIsMuted);
    }

    #endregion
}

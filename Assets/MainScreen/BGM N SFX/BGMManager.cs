using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public enum BGMState
    {
        None,
        MainMenu,
        Tutorial,
        Stage1,
        Stage2,
        Stage3,
        Stage4,
        Ending
    }

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip tutorialBGM;   // 🔥 추가
    public AudioClip stage1BGM;
    public AudioClip stage2BGM;
    public AudioClip stage3BGM;
    public AudioClip stage4BGM;
    public AudioClip endingBGM;

    private AudioSource audioSource;
    private BGMState currentState = BGMState.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===============================
    // 내부 공통 재생
    // ===============================
    private void PlayInternal(AudioClip clip, BGMState state)
    {
        if (clip == null) return;
        if (currentState == state && audioSource.isPlaying) return;

        currentState = state;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
        currentState = BGMState.None;
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
            audioSource.Pause();
    }

    public void Resume()
    {
        if (!audioSource.isPlaying)
            audioSource.UnPause();
    }

    // ===============================
    // 외부 호출용
    // ===============================
    public void PlayMainMenu()
        => PlayInternal(mainMenuBGM, BGMState.MainMenu);

    public void PlayTutorial()
        => PlayInternal(tutorialBGM, BGMState.Tutorial);

    public void PlayStage(int stage)
    {
        switch (stage)
        {
            case 1: PlayInternal(stage1BGM, BGMState.Stage1); break;
            case 2: PlayInternal(stage2BGM, BGMState.Stage2); break;
            case 3: PlayInternal(stage3BGM, BGMState.Stage3); break;
            case 4: PlayInternal(stage4BGM, BGMState.Stage4); break;
        }
    }

    public void PlayEnding()
        => PlayInternal(endingBGM, BGMState.Ending);
}

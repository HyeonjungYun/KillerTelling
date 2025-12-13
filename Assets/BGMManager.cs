using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip stage1BGM;
    public AudioClip stage2BGM;
    public AudioClip stage3BGM;
    public AudioClip stage4BGM;
    public AudioClip endingBGM;

    private AudioSource audioSource;

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
    // 기본 제어
    // ===============================
    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
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
    // 상황별 래퍼
    // ===============================
    public void PlayMainMenu() => Play(mainMenuBGM);

    public void PlayStage(int stage)
    {
        switch (stage)
        {
            case 1: Play(stage1BGM); break;
            case 2: Play(stage2BGM); break;
            case 3: Play(stage3BGM); break;
            case 4: Play(stage4BGM); break;
        }
    }

    public void PlayEnding() => Play(endingBGM);
}

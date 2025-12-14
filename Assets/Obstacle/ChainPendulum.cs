using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ChainPendulum : MonoBehaviour
{
    [Header("Pendulum")]
    public bool active = false;   // ♥ 3장 이상일 때만 흔들기
    public float maxAngle = 25f;
    public float speed = 2.5f;

    [Header("SFX")]
    public AudioClip startSwingSFX;   // 흔들기 시작
    public AudioClip stopSwingSFX;    // 흔들기 종료
    public AudioClip swingLoopSFX;    // 흔들리는 동안 루프
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource audioSource;
    private float baseZ;

    private void Awake()
    {
        baseZ = transform.localEulerAngles.z;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 🔥 2D 사운드
        audioSource.volume = sfxVolume;
    }

    private void Update()
    {
        if (!active) return;

        float angle = Mathf.Sin(Time.time * speed) * maxAngle;
        transform.localRotation = Quaternion.Euler(0, 0, baseZ + angle);
    }

    public void SetActive(bool state)
    {
        if (active == state) return;
        active = state;

        if (active)
        {
            // ▶ 시작 효과음
            if (startSwingSFX != null)
                audioSource.PlayOneShot(startSwingSFX, sfxVolume);

            // ▶ 흔들림 루프
            if (swingLoopSFX != null)
            {
                audioSource.clip = swingLoopSFX;
                audioSource.loop = true;
                audioSource.volume = sfxVolume;
                audioSource.PlayDelayed(0.05f); // 시작음 겹침 방지
            }
        }
        else
        {
            // ■ 루프 중단
            if (audioSource.isPlaying)
            {
                audioSource.loop = false;
                audioSource.Stop();
            }

            // ■ 종료 효과음
            if (stopSwingSFX != null)
                audioSource.PlayOneShot(stopSwingSFX, sfxVolume);
        }
    }

    // 🔥 StageManager / SubmitButton에서 호출용
    public void StopLoopSFX()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    public void ResumeLoopSFX()
    {
        if (active && swingLoopSFX != null)
        {
            audioSource.clip = swingLoopSFX;
            audioSource.loop = true;
            audioSource.volume = sfxVolume;
            audioSource.Play();
        }
    }
}

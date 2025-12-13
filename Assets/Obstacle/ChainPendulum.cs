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

    private AudioSource audioSource;
    private float baseZ;

    private void Awake()
    {
        baseZ = transform.localEulerAngles.z;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;   // 🔥 2D 환경 필수
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
            // ▶ 시작음
            if (startSwingSFX != null)
                audioSource.PlayOneShot(startSwingSFX);

            // ▶ 루프음
            if (swingLoopSFX != null)
            {
                audioSource.clip = swingLoopSFX;
                audioSource.loop = true;
                audioSource.PlayDelayed(0.05f);
            }
        }
        else
        {
            // ■ 루프 중단
            if (audioSource.isPlaying)
                audioSource.Stop();

            // ■ 종료음
            if (stopSwingSFX != null)
                audioSource.PlayOneShot(stopSwingSFX);
        }
    }

    // 🔥 StageManager에서 호출용
    public void StopLoopSFX()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void ResumeLoopSFX()
    {
        if (active && swingLoopSFX != null)
        {
            audioSource.clip = swingLoopSFX;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MovingTargetObstacle : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool active = false;

    public float moveAmplitude = 0.3f;  // 좌우 이동 범위
    public float moveSpeed = 1.2f;      // 진동 속도

    public float rotateAmplitude = 5f;  // 회전 범위 (degrees)
    public float rotateSpeed = 1f;      // 회전 속도

    private Vector3 initialPos;
    private Quaternion initialRot;

    // 🔊 (추가) SFX
    [Header("SFX")]
    public AudioClip startSFX;     // 작동 시작음
    public AudioClip loopSFX;      // 움직임 루프음
    public AudioClip stopSFX;      // 정지음
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;

        // 🔊 AudioSource 준비
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.volume = sfxVolume;
    }

    /// <summary>
    /// 외부에서 반드시 이 함수로만 상태 변경 (사운드/위치 원복 포함)
    /// </summary>
    public void SetActive(bool state)
    {
        if (active == state) return;
        active = state;

        if (active)
        {
            // ▶ 시작음
            if (startSFX != null)
                audioSource.PlayOneShot(startSFX, sfxVolume);

            // ▶ 루프음
            if (loopSFX != null)
            {
                audioSource.clip = loopSFX;
                audioSource.loop = true;
                audioSource.volume = sfxVolume;
                audioSource.Play();
            }
        }
        else
        {
            // ■ 루프 중단
            if (audioSource != null && audioSource.isPlaying)
                audioSource.Stop();

            // ■ 정지음
            if (stopSFX != null)
                audioSource.PlayOneShot(stopSFX, sfxVolume);

            // 위치/회전 원복
            transform.localPosition = initialPos;
            transform.localRotation = initialRot;
        }
    }

    private void Update()
    {
        if (!active) return;

        float t = Time.time;

        // 좌우 이동
        float offsetX = Mathf.Sin(t * moveSpeed) * moveAmplitude;

        // 회전
        float rotZ = Mathf.Sin(t * rotateSpeed) * rotateAmplitude;

        transform.localPosition = initialPos + new Vector3(offsetX, 0f, 0f);
        transform.localRotation = initialRot * Quaternion.Euler(0f, 0f, rotZ);
    }

    // 🔥 StageManager에서 호출용 (Submit 시 루프 끊기 등)
    public void StopLoopSFX()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void ResumeLoopSFX()
    {
        if (active && loopSFX != null)
        {
            audioSource.clip = loopSFX;
            audioSource.loop = true;
            audioSource.volume = sfxVolume;
            audioSource.Play();
        }
    }
}

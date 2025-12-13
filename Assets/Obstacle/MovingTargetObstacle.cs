using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MovingTargetObstacle : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool active = false;

    [Header("SFX")]
    public AudioClip startSFX;     // 작동 시작음
    public AudioClip loopSFX;      // 움직임 루프음
    public AudioClip stopSFX;      // 정지음

    [Header("Movement")]
    public float moveAmplitude = 0.3f;   // 좌우 이동 범위
    public float moveSpeed = 1.2f;       // 진동 속도
    public float rotateAmplitude = 5f;   // 회전 범위 (degrees)
    public float rotateSpeed = 1f;       // 회전 속도

    private AudioSource audioSource;
    private Vector3 initialPos;
    private Quaternion initialRot;

    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;   // 🔥 UI / 2D 환경이면 필수
        audioSource.volume = 1f;
    }

    /// <summary>
    /// 외부에서 반드시 이 함수로만 상태 변경
    /// </summary>
    public void SetActive(bool state)
    {
        if (active == state) return;

        active = state;

        if (active)
        {
            // ▶ 시작음
            if (startSFX != null)
                audioSource.PlayOneShot(startSFX);

            // ▶ 루프음
            if (loopSFX != null)
            {
                audioSource.clip = loopSFX;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            // ■ 루프 중단
            if (audioSource.isPlaying)
                audioSource.Stop();

            // ■ 정지음
            if (stopSFX != null)
                audioSource.PlayOneShot(stopSFX);

            // 위치 원복
            transform.localPosition = initialPos;
            transform.localRotation = initialRot;
        }
    }

    private void Update()
    {
        if (!active) return;

        float t = Time.time;

        float offsetX = Mathf.Sin(t * moveSpeed) * moveAmplitude;
        float rotZ = Mathf.Sin(t * rotateSpeed) * rotateAmplitude;

        transform.localPosition = initialPos + new Vector3(offsetX, 0f, 0f);
        transform.localRotation = initialRot * Quaternion.Euler(0f, 0f, rotZ);
    }

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
            audioSource.Play();
        }
    }

}

using System.Collections;
using UnityEngine;
using System; // 🔥 Action 사용을 위해 필수

public class EnergyDrinkMover : MonoBehaviour
{
    [Header("Model")]
    public Transform canModel;

    [Header("World Positions")]
    public Vector3 idlePosition = new Vector3(1.6f, 1.05f, -3f);
    public Vector3 drinkPosition = new Vector3(0.4f, 1.05f, -5f);

    [Header("Animation Settings")]
    public float moveSpeed = 2.5f;
    public float tiltAngle = -60f;
    public float tiltDuration = 0.6f;
    public float holdDuration = 0.4f;
    public float returnSpeed = 2.0f;

    [Header("Height Adjustment")]
    public float drinkLiftOffset = 0.4f;

    // 🔊 (추가) SFX / Loop
    [Header("SFX")]
    public AudioClip pickUpSFX;      // 연출 시작
    public AudioClip moveSFX;        // (선택) 이동 중 루프
    public AudioClip tiltSFX;        // 기울이기
    public AudioClip drinkLoopSFX;   // 마시는 동안 루프
    public AudioClip placeDownSFX;   // 내려놓기
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource audioSource;

    private Coroutine routine;
    private bool isPlaying = false;

    private void Reset()
    {
        if (canModel == null && transform.childCount > 0)
            canModel = transform.GetChild(0);
    }

    private void Awake()
    {
        if (canModel == null && transform.childCount > 0)
            canModel = transform.GetChild(0);

        transform.position = idlePosition;

        // 🔊 AudioSource 준비
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f; // 2D
        audioSource.volume = sfxVolume;
    }

    private void PlayOneShotSafe(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip, sfxVolume);
    }

    private void StartLoop(AudioClip clip, float delay = 0f)
    {
        if (clip == null || audioSource == null) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = sfxVolume;
        if (delay > 0f) audioSource.PlayDelayed(delay);
        else audioSource.Play();
    }

    private void StopLoop()
    {
        if (audioSource == null) return;
        if (audioSource.loop && audioSource.isPlaying)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// 연출 실행. onComplete는 연출이 끝난 후 실행할 함수(입력 차단 해제 등).
    /// </summary>
    public void PlayDrinkOnce(Action onComplete = null)
    {
        if (isPlaying) return;

        // 🔊 연출 시작 SFX
        PlayOneShotSafe(pickUpSFX);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(DrinkRoutine(onComplete));
    }

    private IEnumerator DrinkRoutine(Action onComplete)
    {
        isPlaying = true;
        Transform target = transform;

        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;

        // 1. 플레이어 앞으로 이동
        Vector3 drinkPeakPosition = new Vector3(
            drinkPosition.x,
            drinkPosition.y + drinkLiftOffset,
            drinkPosition.z
        );

        // 🔊 이동 루프(선택)
        if (moveSFX != null)
            StartLoop(moveSFX);

        while (Vector3.Distance(target.position, drinkPeakPosition) > 0.01f)
        {
            target.position = Vector3.Lerp(target.position, drinkPeakPosition, Time.deltaTime * moveSpeed);
            target.rotation = Quaternion.Slerp(target.rotation, Quaternion.identity, Time.deltaTime * moveSpeed * 0.5f);
            yield return null;
        }
        target.position = drinkPeakPosition;

        // 🔇 이동 루프 종료
        StopLoop();

        // 🔊 기울이기 SFX
        PlayOneShotSafe(tiltSFX);

        // 2. 기울여서 마시기
        Quaternion tiltStart = target.rotation;
        Quaternion tiltEnd = tiltStart * Quaternion.Euler(tiltAngle, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            target.rotation = Quaternion.Slerp(tiltStart, tiltEnd, t);
            yield return null;
        }

        // 🔊 마시는 루프
        if (drinkLoopSFX != null)
            StartLoop(drinkLoopSFX);

        yield return new WaitForSeconds(holdDuration);

        // 🔇 마시는 루프 종료
        StopLoop();

        // 3. 원래 위치로 복귀
        t = 0f;
        Quaternion backStartRot = target.rotation;
        Quaternion backEndRot = startRot;

        while (Vector3.Distance(target.position, idlePosition) > 0.01f ||
               Quaternion.Angle(target.rotation, backEndRot) > 0.5f)
        {
            target.position = Vector3.Lerp(target.position, idlePosition, Time.deltaTime * returnSpeed);
            target.rotation = Quaternion.Slerp(target.rotation, backEndRot, Time.deltaTime * returnSpeed);
            yield return null;
        }

        target.position = idlePosition;
        target.rotation = backEndRot;

        // 🔊 내려놓기 SFX
        PlayOneShotSafe(placeDownSFX);

        isPlaying = false;

        // 🔥 [핵심] 연출 종료 콜백 실행 -> 매니저에게 "입력 풀어도 돼"라고 알림
        if (onComplete != null)
            onComplete.Invoke();
    }

    // ✅ (선택) 외부에서 강제 중단 시 루프 사운드 정리용
    private void OnDisable()
    {
        StopLoop();
    }
}

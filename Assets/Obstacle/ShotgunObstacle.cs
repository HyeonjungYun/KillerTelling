using System.Collections;
using UnityEngine;

public class ShotgunObstacle : MonoBehaviour
{
    [Header("Move Settings")]
    public Transform model;              // 샷건 모델(움직일 대상)
    public Transform idlePosition;       // 테이블 위 자리
    public Transform obstaclePosition;   // 방해물 자리(들린 상태)
    public float moveSpeed = 4f;

    private bool isActive = false;

    [Header("Collision Settings")]
    public Collider obstacleCollider;    // 실제 카드와 부딪힐 콜라이더(없으면 자동으로 찾음)

    // ✅ (추가) 설명 Hover 전용 콜라이더 (항상 켜둠 / 레이어 고정)
    [Header("Hover Info (Always On)")]
    public Collider hoverInfoCollider;   // ObstacleHover3D가 붙어있는 콜라이더 추천
    public string hoverLayerName = "Obstacle"; // hover 이벤트가 먹는 레이어

    private int obstacleLayer;
    private int ignoreCardLayer;
    private int hoverLayer;

    // 🔊 (추가) SFX
    [Header("SFX")]
    public AudioClip activateSFX;    // 올라올 때
    public AudioClip deactivateSFX;  // 내려갈 때
    public AudioClip moveLoopSFX;    // (선택) 움직이는 동안 루프
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (model == null)
            model = transform;

        if (obstacleCollider == null)
            obstacleCollider = GetComponentInChildren<Collider>(true);

        obstacleLayer = LayerMask.NameToLayer("Obstacle");
        ignoreCardLayer = LayerMask.NameToLayer("IgnoreCard");
        hoverLayer = LayerMask.NameToLayer(hoverLayerName);

        // hoverInfoCollider 자동 추정(없으면: obstacleCollider와 다른 collider 찾기)
        if (hoverInfoCollider == null)
        {
            var cols = model.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                if (c == null) continue;
                if (c == obstacleCollider) continue;
                hoverInfoCollider = c;
                break;
            }
        }

        // 🔊 AudioSource 준비
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = sfxVolume;

        // ✅ hover는 항상 켜고, 레이어도 고정
        ApplyHoverInfoState();

        // 처음에는 비활성 상태로 시작 (충돌 X)
        SetActiveState(false);
    }

    private void ApplyHoverInfoState()
    {
        if (hoverInfoCollider != null)
        {
            hoverInfoCollider.enabled = true; // 항상 켜둠
            if (hoverLayer >= 0)
                SetLayerRecursively(hoverInfoCollider.gameObject, hoverLayer); // 항상 Obstacle(또는 지정 레이어)
        }
    }

    private void PlayOneShotSafe(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.volume = sfxVolume;
        audioSource.loop = false;
        audioSource.PlayOneShot(clip);
    }

    private void StopLoopIfPlaying()
    {
        if (audioSource == null) return;
        if (audioSource.loop)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// 무덤 조건에 따라 CardGraveyardManager 에서 호출됨
    /// </summary>
    public void SetActiveState(bool active)
    {
        if (isActive == active) return;
        isActive = active;

        // ✅ hover는 항상 유지
        ApplyHoverInfoState();

        // 🔊 상태 전환 SFX
        if (active) PlayOneShotSafe(activateSFX);
        else PlayOneShotSafe(deactivateSFX);

        // 1) 충돌/레이어 상태 먼저 적용
        ApplyCollisionState(active);

        // 2) 그 다음 위치 이동 코루틴 실행
        StopAllCoroutines();
        StartCoroutine(active ? MoveUp() : MoveDown());

        Debug.Log($"[ShotgunObstacle] active = {active}, modelLayer = {LayerMask.LayerToName(model.gameObject.layer)}");
    }

    // ------------------------------------------------------------
    //   이동 코루틴
    // ------------------------------------------------------------
    private IEnumerator MoveUp()
    {
        if (model == null || obstaclePosition == null) yield break;

        if (moveLoopSFX != null && audioSource != null)
        {
            audioSource.volume = sfxVolume;
            audioSource.clip = moveLoopSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        while (Vector3.Distance(model.position, obstaclePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(model.position, obstaclePosition.position, Time.deltaTime * moveSpeed);
            model.rotation = Quaternion.Slerp(model.rotation, obstaclePosition.rotation, Time.deltaTime * moveSpeed);
            yield return null;
        }

        StopLoopIfPlaying();
    }

    private IEnumerator MoveDown()
    {
        if (model == null || idlePosition == null) yield break;

        StopLoopIfPlaying();

        while (Vector3.Distance(model.position, idlePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(model.position, idlePosition.position, Time.deltaTime * moveSpeed);
            model.rotation = Quaternion.Slerp(model.rotation, idlePosition.rotation, Time.deltaTime * moveSpeed);
            yield return null;
        }
    }

    // ------------------------------------------------------------
    //   레이어/콜라이더 상태 제어
    // ------------------------------------------------------------
    private void ApplyCollisionState(bool active)
    {
        // ✅ 실제 충돌만 on/off
        if (obstacleCollider != null)
            obstacleCollider.enabled = active;

        // ✅ 레이어 변경도 "obstacleCollider 쪽(충돌용)"에만 적용
        int targetLayer = active ? obstacleLayer : ignoreCardLayer;
        if (targetLayer >= 0 && obstacleCollider != null)
        {
            SetLayerRecursively(obstacleCollider.gameObject, targetLayer);
        }

        // ✅ hoverInfoCollider는 항상 Obstacle 레이어 유지 (위에서 ApplyHoverInfoState로 처리)
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;
        foreach (Transform t in obj.transform.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    private void OnDisable()
    {
        // 혹시 정보창이 떠있으면 숨김(안전)
        if (ObstacleInfoUI.Instance != null)
            ObstacleInfoUI.Instance.Hide();
    }
}

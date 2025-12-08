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

    private int obstacleLayer;
    private int ignoreCardLayer;

    private void Awake()
    {
        // 기본 참조 세팅
        if (model == null)
            model = transform;

        if (obstacleCollider == null)
            obstacleCollider = GetComponentInChildren<Collider>();

        obstacleLayer = LayerMask.NameToLayer("Obstacle");
        ignoreCardLayer = LayerMask.NameToLayer("IgnoreCard");

        // 처음에는 항상 비활성 상태로 시작 (충돌 X)
        SetActiveState(false);
    }

    /// <summary>
    /// 무덤 조건에 따라 CardGraveyardManager 에서 호출됨
    /// spade >= 3 이면 active=true, 아니면 false 로 들어옴
    /// </summary>
    public void SetActiveState(bool active)
    {
        if (isActive == active) return;
        isActive = active;

        // 1) 충돌/레이어 상태 먼저 적용
        ApplyCollisionState(active);

        // 2) 그 다음 위치 이동 코루틴 실행
        StopAllCoroutines();
        StartCoroutine(active ? MoveUp() : MoveDown());

        Debug.Log($"[ShotgunObstacle] active = {active}, layer = {LayerMask.LayerToName(model.gameObject.layer)}");
    }

    // ------------------------------------------------------------
    //   이동 코루틴 (기존 코드 그대로)
    // ------------------------------------------------------------
    private IEnumerator MoveUp()
    {
        if (model == null || obstaclePosition == null) yield break;

        while (Vector3.Distance(model.position, obstaclePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(
                model.position,
                obstaclePosition.position,
                Time.deltaTime * moveSpeed
            );

            model.rotation = Quaternion.Slerp(
                model.rotation,
                obstaclePosition.rotation,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }
    }

    private IEnumerator MoveDown()
    {
        if (model == null || idlePosition == null) yield break;

        while (Vector3.Distance(model.position, idlePosition.position) > 0.01f)
        {
            model.position = Vector3.Lerp(
                model.position,
                idlePosition.position,
                Time.deltaTime * moveSpeed
            );

            model.rotation = Quaternion.Slerp(
                model.rotation,
                idlePosition.rotation,
                Time.deltaTime * moveSpeed
            );

            yield return null;
        }
    }

    // ------------------------------------------------------------
    //   레이어/콜라이더 상태 제어
    // ------------------------------------------------------------
    private void ApplyCollisionState(bool active)
    {
        // 콜라이더 on/off
        if (obstacleCollider != null)
            obstacleCollider.enabled = active;

        // 레이어 Obstacle / IgnoreCard 전환
        int targetLayer = active ? obstacleLayer : ignoreCardLayer;
        if (targetLayer >= 0)
            SetLayerRecursively(model.gameObject, targetLayer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform t in obj.transform.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}

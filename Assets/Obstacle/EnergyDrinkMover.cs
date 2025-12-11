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
    }

    /// <summary>
    /// 연출 실행. onComplete는 연출이 끝난 후 실행할 함수(입력 차단 해제 등).
    /// </summary>
    public void PlayDrinkOnce(Action onComplete = null)
    {
        if (isPlaying) return;

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

        while (Vector3.Distance(target.position, drinkPeakPosition) > 0.01f)
        {
            target.position = Vector3.Lerp(target.position, drinkPeakPosition, Time.deltaTime * moveSpeed);
            target.rotation = Quaternion.Slerp(target.rotation, Quaternion.identity, Time.deltaTime * moveSpeed * 0.5f);
            yield return null;
        }
        target.position = drinkPeakPosition;

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

        yield return new WaitForSeconds(holdDuration);

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

        isPlaying = false;

        // 🔥 [핵심] 연출 종료 콜백 실행 -> 매니저에게 "입력 풀어도 돼"라고 알림
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
}
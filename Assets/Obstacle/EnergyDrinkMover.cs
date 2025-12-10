using System.Collections;
using UnityEngine;

public class EnergyDrinkMover : MonoBehaviour
{
    [Header("Model (캔 Mesh가 붙은 Transform)")]
    public Transform canModel;          // 있으면 할당, 없어도 동작은 transform 기준으로 가능

    [Header("World Positions")]
    // ✅ “원래 자리” — 무조건 여기로 돌아오게 할 것
    public Vector3 idlePosition = new Vector3(1.6f, 1.05f, -3f);

    // 플레이어 앞 위치 (Z만 조절용, 실제로는 이 값 + lift 로 이동)
    public Vector3 drinkPosition = new Vector3(0.4f, 1.05f, -5f);

    [Header("Animation Settings")]
    public float moveSpeed = 2.5f;      // 이동 속도
    public float tiltAngle = -60f;      // 마실 때 앞으로 기울이는 각도
    public float tiltDuration = 0.6f;   // 기울이는 시간
    public float holdDuration = 0.4f;   // 기울인 상태 유지 시간
    public float returnSpeed = 2.0f;    // 원위치로 되돌아오는 속도

    [Header("Height Adjustment")]
    public float drinkLiftOffset = 0.4f;  // 플레이어 앞에서 얼마나 들어 올릴지

    private Coroutine routine;
    private bool isPlaying = false;

    private void Reset()
    {
        // 에디터에서 Add Component 했을 때 자동으로 자식 찾기
        if (canModel == null && transform.childCount > 0)
            canModel = transform.GetChild(0);
    }

    private void Awake()
    {
        if (canModel == null && transform.childCount > 0)
            canModel = transform.GetChild(0);

        // ✅ 시작할 때 항상 idlePosition에 배치
        transform.position = idlePosition;
        // 필요하면 기본 회전값도 여기서 정해줄 수 있음
        // transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 다이아가 4장 이상이 되었을 때 한 번 호출해주면 됨
    /// </summary>
    public void PlayDrinkOnce()
    {
        if (isPlaying) return;      // 이미 연출 중이면 무시

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(DrinkRoutine());
    }

    private IEnumerator DrinkRoutine()
    {
        isPlaying = true;

        // 🔥 실제로 움직이고 도는 대상은 오브젝트 자체
        Transform target = transform;

        // -----------------------------
        // 0) 시작 위치 / 회전 저장
        // -----------------------------
        Vector3 startPos = target.position;       // 보통 idlePosition과 같을 것
        Quaternion startRot = target.rotation;

        // -----------------------------
        // 1) 테이블 → 플레이어 앞 + 조금 위로 이동
        // -----------------------------
        Vector3 drinkPeakPosition = new Vector3(
            drinkPosition.x,
            drinkPosition.y + drinkLiftOffset,    // 🔥 여기서 Y를 살짝 올림
            drinkPosition.z
        );

        while (Vector3.Distance(target.position, drinkPeakPosition) > 0.01f)
        {
            target.position = Vector3.Lerp(target.position, drinkPeakPosition, Time.deltaTime * moveSpeed);

            // 자연스럽게 세워지게 하고 싶으면 identity 혹은 원하는 회전값으로 보간
            target.rotation = Quaternion.Slerp(target.rotation, Quaternion.identity, Time.deltaTime * moveSpeed * 0.5f);

            yield return null;
        }

        target.position = drinkPeakPosition;

        // -----------------------------
        // 2) 기울여서 마시는 연출
        // -----------------------------
        Quaternion tiltStart = target.rotation;
        Quaternion tiltEnd = tiltStart * Quaternion.Euler(tiltAngle, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / tiltDuration;
            target.rotation = Quaternion.Slerp(tiltStart, tiltEnd, t);
            yield return null;
        }

        // 잠깐 유지
        yield return new WaitForSeconds(holdDuration);

        // -----------------------------
        // 3) 다시 세우고, “꼭” idlePosition으로 복귀
        // -----------------------------
        t = 0f;
        Quaternion backStartRot = target.rotation;
        Quaternion backEndRot = startRot;   // 시작 회전으로 복귀

        while (Vector3.Distance(target.position, idlePosition) > 0.01f ||
               Quaternion.Angle(target.rotation, backEndRot) > 0.5f)
        {
            target.position = Vector3.Lerp(target.position, idlePosition, Time.deltaTime * returnSpeed);
            target.rotation = Quaternion.Slerp(target.rotation, backEndRot, Time.deltaTime * returnSpeed);
            yield return null;
        }

        // 최종 보정
        target.position = idlePosition;
        target.rotation = backEndRot;

        isPlaying = false;
    }
}

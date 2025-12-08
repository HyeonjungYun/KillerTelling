using System.Collections;
using UnityEngine;

public class EnergyDrinkMover : MonoBehaviour
{
    [Header("Model (캔 Mesh가 붙은 Transform)")]
    public Transform canModel;          // <- EnergyDrink_High__326_Tris_ 할당

    [Header("World Positions")]
    // 시작 위치(테이블 위) – 현재 월드 좌표
    public Vector3 idlePosition = new Vector3(1.6f, 1.05f, -3f);

    // 플레이어 앞 위치 – 현재 월드 좌표
    public Vector3 drinkPosition = new Vector3(0.4f, 1.05f, -5f);

    [Header("Animation Settings")]
    public float moveSpeed = 2.5f;      // 이동 속도
    public float tiltAngle = -60f;      // 마실 때 앞으로 기울이는 각도
    public float tiltDuration = 0.6f;   // 기울이는 시간
    public float holdDuration = 0.4f;   // 기울인 상태 유지 시간
    public float returnSpeed = 2.0f;    // 원위치로 되돌아오는 속도

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
        if (canModel == null)
            canModel = transform.GetChild(0);

        // 처음 배치된 월드 위치를 idlePosition으로 쓴다면 아래 두 줄로 덮어도 됨
        idlePosition = transform.position;
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

        Transform target = (canModel != null) ? canModel : transform;

        // -----------------------------
        // 1) 테이블 → 플레이어 앞 이동
        // -----------------------------
        Vector3 startPos = target.position;
        Quaternion startRot = target.rotation;

        while (Vector3.Distance(target.position, drinkPosition) > 0.01f)
        {
            target.position = Vector3.Lerp(target.position, drinkPosition, Time.deltaTime * moveSpeed);
            target.rotation = Quaternion.Slerp(target.rotation, Quaternion.identity, Time.deltaTime * moveSpeed * 0.5f);
            yield return null;
        }

        target.position = drinkPosition;

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
        // 3) 다시 세우고 테이블 위치로 복귀
        // -----------------------------
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
    }
}

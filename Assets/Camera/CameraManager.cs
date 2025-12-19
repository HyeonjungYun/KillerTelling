using UnityEngine;
using System.Collections;

public class CameraRotator : MonoBehaviour
{
    public static CameraRotator instance;
    public Camera mainCamera;

    private void Awake()
    {
        if (instance == null) instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    // 외부 호출용 함수: "targetAnchor의 각도로 duration초 동안 회전해라"
    public void RotateSmoothly(Transform targetAnchor, float duration)
    {
        // 이미 돌고 있는 코루틴이 있다면 멈추고 새로 시작 (중복 실행 방지)
        StopAllCoroutines();
        StartCoroutine(RotationRoutine(targetAnchor, duration));
    }

    IEnumerator RotationRoutine(Transform target, float duration)
    {
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion endRot = target.rotation;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // [전공생 Tip] SmoothStep 공식 (t * t * (3f - 2f * t))
            // 단순히 선형(Linear)으로 회전하면 기계적으로 보입니다.
            // 이 공식을 쓰면 '천천히 출발 -> 중간 가속 -> 천천히 도착'하는 Ease-In-Out 효과가 적용됩니다.
            t = t * t * (3f - 2f * t);

            // 핵심: 현재 각도(start)에서 목표 각도(end)로 t만큼 구면 보간
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // 오차 보정: 루프가 끝나면 정확히 목표 각도로 고정
        mainCamera.transform.rotation = endRot;
    }
}
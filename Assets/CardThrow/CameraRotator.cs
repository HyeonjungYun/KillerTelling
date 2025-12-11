using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    public float defaultX = 25f;
    public float focusX = 0f;
    public float speed = 5f; // 복귀 속도를 조금 더 빠르게(3->5) 추천

    private float currentX;
    private float targetX;

    private Vector3 currentBaseRotation;
    private bool isRotatorActive = false;

    private void Awake()
    {
        // 게임 시작 시점의 각도를 기본값으로 잡음
        // (만약 인스펙터 창에서 설정한 25도를 쓰고 싶다면 이 줄을 지우세요)
        defaultX = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        // 활성화되지 않았으면 작동 안 함
        if (!isRotatorActive) return;

        // 1. 부드럽게 각도 변경 (Lerp)
        currentX = Mathf.LerpAngle(currentX, targetX, Time.deltaTime * speed);

        // 2. 회전 적용
        Vector3 rot = currentBaseRotation;
        rot.x = currentX;
        transform.eulerAngles = rot;

        // (선택 사항) 거의 다 돌아왔으면 불필요한 연산 방지를 위해 꺼도 됨
        // 하지만 계속 켜둬도 성능에 큰 지장은 없습니다.
    }

    public void LookFront()
    {
        // 1. CameraDirector 멈춤
        if (CameraDirector.Instance != null)
            CameraDirector.Instance.StopTracking();

        // 2. 현재 Y, Z 각도 기억 (고개만 끄덕이기 위해)
        currentBaseRotation = transform.eulerAngles;
        currentX = transform.eulerAngles.x;

        // 3. 목표 변경 -> 0도 (정면)
        targetX = focusX;

        // 4. 엔진 가동!
        isRotatorActive = true;
    }

    public void LookDefault()
    {
        // 1. 목표 변경 -> 25도 (원래 각도)
        targetX = defaultX;

        // ★ 중요: 여기서 isRotatorActive = false를 하면 안 됩니다!
        // 돌아가는 애니메이션이 끝날 때까지 켜둬야 합니다.
        isRotatorActive = true;
    }
}
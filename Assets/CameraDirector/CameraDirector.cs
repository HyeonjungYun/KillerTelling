using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    public static CameraDirector Instance;

    [Header("Target Camera")]
    public Camera mainCamera;

    [Header("Settings")]
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 5.0f;

    // 현재 목표지점
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // ★ 추가된 변수: 카메라가 타겟을 따라가야 하는지 여부
    private bool isFollowingTarget = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera != null)
        {
            targetPosition = mainCamera.transform.position;
            targetRotation = mainCamera.transform.rotation;
        }
    }

    private void LateUpdate()
    {
        if (!mainCamera) return;

        // ★ 수정된 부분: 추적 모드가 꺼져있으면(false) 아무것도 안 함 (기존 로직이 작동하도록)
        if (!isFollowingTarget) return;

        // 거리/각도 차이가 있을 때만 이동
        if (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.01f ||
            Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetRotation,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    // ================================================================
    // 기능 1. 이동 명령 (이때는 자동으로 추적 모드를 켭니다)
    // ================================================================
    public void MoveToTarget(Transform targetAnchor)
    {
        if (targetAnchor == null) return;

        // 이동 명령이 내려오면 다시 이 스크립트가 제어권을 가짐
        isFollowingTarget = true;

        targetPosition = targetAnchor.position;
        targetRotation = targetAnchor.rotation;
    }

    // ================================================================
    // ★ 기능 2. 추적 중지 (카드를 던질 때 호출!)
    // ================================================================
    public void StopTracking()
    {
        // 이 스크립트는 더 이상 카메라를 건드리지 않음 -> 기존 카드 로직이 작동함
        isFollowingTarget = false;
    }
}
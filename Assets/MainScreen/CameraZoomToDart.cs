using UnityEngine;

public class CameraZoomToDart : MonoBehaviour
{
    [Header("Settings")]
    public float zoomInPos = -6.0f;   // 줌 인(가까이) 했을 때 Z 위치
    public float zoomOutPos = 3.5f;   // 줌 아웃(멀리) 했을 때 Z 위치

    // 내부 상태 변수
    private float defaultY;           // Y 위치 고정용
    private bool isZoomedIn = false;  // 현재 줌 상태 (true면 줌인 상태)
    private bool zoomLocked = false;  // 조준 중 잠금 여부

    void Start()
    {
        // 시작할 때 Y 높이 저장
        defaultY = transform.position.y;

        // 시작할 때 줌 아웃 상태라고 가정하고 위치 잡기
        isZoomedIn = false;
        SetPositionZ(zoomOutPos);
    }

    void Update()
    {
        if (zoomLocked) return; // 🔒 잠겨있으면 입력 무시

        // R 키 하나로 토글 (Toggle)
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 상태 뒤집기 (true -> false, false -> true)
            isZoomedIn = !isZoomedIn;

            if (isZoomedIn)
            {
                SetPositionZ(zoomInPos); // 줌 인 위치로 이동
            }
            else
            {
                SetPositionZ(zoomOutPos); // 줌 아웃 위치로 이동
            }
        }
    }

    // [내부 헬퍼 함수] Z값만 변경하여 즉시 이동
    private void SetPositionZ(float targetZ)
    {
        transform.position = new Vector3(
            transform.position.x,
            defaultY,
            targetZ
        );
    }

    // =========================================================
    // [외부 공개 함수 - 이름 유지]
    // =========================================================

    public void LockZoom()
    {
        zoomLocked = true;
    }

    public void UnlockZoom()
    {
        zoomLocked = false;
    }

    public void ResetZoom()
    {
        // 리셋 시 줌 아웃 상태(기본)로 되돌림
        isZoomedIn = false;
        SetPositionZ(zoomOutPos);
        zoomLocked = false;
    }
}
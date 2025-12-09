using UnityEngine;

public class CameraZoomToDart : MonoBehaviour
{
    public float zoomSpeed = 6f;

    // Z 값 범위
    public float zoomMin = -7.6f;   // 제일 가까운 쪽 (카메라가 앞으로)
    public float zoomMax = 3.5f;    // 제일 먼 쪽

    private float defaultZ;         // 시작 Z 저장
    private float defaultY;         // 시작 Y 저장 ⭐
    private bool zoomLocked = false;  // 조커가 조준 중일 때 true

    void Start()
    {
        Vector3 pos = transform.position;
        defaultZ = pos.z;
        defaultY = pos.y;   // ▶ Y는 항상 이 높이를 유지하게 만들 예정
    }

    void Update()
    {
        if (zoomLocked) return; // 🔒 잠겨있으면 R/E 입력 무시

        float z = transform.position.z;

        // R = 앞으로 다가가기 (Z 감소)
        if (Input.GetKey(KeyCode.R))
            z -= zoomSpeed * Time.deltaTime;

        // E = 뒤로 물러나기 (Z 증가)
        if (Input.GetKey(KeyCode.E))
            z += zoomSpeed * Time.deltaTime;

        // 범위 제한
        z = Mathf.Clamp(z, zoomMin, zoomMax);

        // ⭐ Y는 항상 defaultY 고정 → 위로 떠버리는 현상 방지
        transform.position = new Vector3(
            transform.position.x,
            defaultY,
            z
        );
    }

    // 🔒 조커가 “던지기 모드” 들어갈 때 호출
    public void LockZoom()
    {
        zoomLocked = true;
    }

    // 🔓 카드가 벽에 박혀서 던지기 끝났을 때 호출
    public void UnlockZoom()
    {
        zoomLocked = false;
    }

    // 🔄 스테이지 전환 시 Z/Y를 초기 상태로 되돌리고 싶을 때
    public void ResetZoom()
    {
        Vector3 pos = transform.position;
        transform.position = new Vector3(
            pos.x,
            defaultY,   // 원래 높이
            defaultZ    // 원래 거리
        );
    }
}

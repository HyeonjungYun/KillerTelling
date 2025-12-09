using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    public float defaultX = 25f;  // 기본 X 각도 (Awake에서 덮어씀)
    public float focusX = 0f;     // 던지기 모드일 때 보는 X 각도
    public float speed = 3f;

    private float currentX;
    private float targetX;

    private Vector3 defaultEuler;   // ⭐ 초기 전체 회전 값 보관 (Y/Z 포함)
    private bool initialized = false;

    private void Awake()
    {
        // 🔥 카메라의 현재 회전값을 저장
        defaultEuler = transform.eulerAngles;

        currentX = defaultEuler.x;
        targetX = currentX;

        // 🔥 defaultX를 현재 X 회전으로 덮어쓰기 (씬에서 배치한 각도를 기본값으로)
        defaultX = currentX;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        currentX = Mathf.LerpAngle(currentX, targetX, Time.deltaTime * speed);

        // ⭐ Y/Z는 항상 defaultEuler 기준으로 유지
        Vector3 rot = defaultEuler;
        rot.x = currentX;

        transform.eulerAngles = rot;
    }

    public void LookFront()
    {
        if (!initialized) return;
        targetX = focusX;
    }

    public void LookDefault()
    {
        if (!initialized) return;
        targetX = defaultX;
    }
}

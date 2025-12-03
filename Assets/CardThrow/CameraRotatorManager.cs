using UnityEngine;

public class CameraRotatorManager : MonoBehaviour
{
    public float defaultX = 25f;
    public float focusX = 0f;
    public float speed = 3f;

    private float currentX;
    private float targetX;

    private bool initialized = false;

    private void Awake()
    {
        // 🔥 카메라의 현재 x 각도를 저장
        currentX = transform.eulerAngles.x;
        targetX = currentX;

        // 🔥 defaultX 값을 현재 카메라 각도로 자동 맞춤 (안전장치)
        defaultX = currentX;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        currentX = Mathf.LerpAngle(currentX, targetX, Time.deltaTime * speed);

        Vector3 rot = transform.eulerAngles;
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

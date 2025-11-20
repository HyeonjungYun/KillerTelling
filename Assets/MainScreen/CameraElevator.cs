using UnityEngine;

public class CameraElevator : MonoBehaviour
{
    public static CameraElevator Instance;

    public float moveUpAmount = 1.0f;
    public float moveSpeedUp = 5f;
    public float moveSpeedDown = 2.5f;

    private Vector3 originalPos;
    private Vector3 targetPos;
    private bool isMovingDownDelayed = false;

    private void Awake()
    {
        Instance = this;

        originalPos = transform.localPosition;
        targetPos = originalPos;
    }

    private void Update()
    {
        float speed = (targetPos.y > transform.localPosition.y) ? moveSpeedUp : moveSpeedDown;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * speed
        );
    }

    // ---------------------------
    // 카메라 올리기
    // ---------------------------
    public void RaiseCamera()
    {
        if (isMovingDownDelayed) return;
        targetPos = originalPos + new Vector3(0, moveUpAmount, 0);
    }

    // ---------------------------
    // 즉시 내려오기
    // ---------------------------
    public void ResetCamera()
    {
        targetPos = originalPos;
    }

    // ---------------------------
    // 🔥 딜레이 후 내려오기 (새 기능)
    // ---------------------------
    public void ResetCameraDelayed(float delay)
    {
        if (isMovingDownDelayed) return;
        Instance.StartCoroutine(DelayRoutine(delay));
    }

    private System.Collections.IEnumerator DelayRoutine(float delay)
    {
        isMovingDownDelayed = true;
        yield return new WaitForSeconds(delay);

        targetPos = originalPos;
        isMovingDownDelayed = false;
    }

    // 현재 카메라 상승 비율
    public float Height01
    {
        get
        {
            float h = transform.localPosition.y - originalPos.y;
            return Mathf.Clamp01(h / moveUpAmount);
        }
    }
}

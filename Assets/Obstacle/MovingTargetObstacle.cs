using UnityEngine;

public class MovingTargetObstacle : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool active = false;

    public float moveAmplitude = 0.3f;  // 좌우 이동 범위
    public float moveSpeed = 1.2f;      // 진동 속도

    public float rotateAmplitude = 5f;  // 회전 범위 (degrees)
    public float rotateSpeed = 1f;      // 회전 속도

    private Vector3 initialPos;
    private Quaternion initialRot;

    private void Awake()
    {
        initialPos = transform.localPosition;
        initialRot = transform.localRotation;
    }

    private void Update()
    {
        if (!active) return;

        float t = Time.time;

        // 좌우 이동
        float offsetX = Mathf.Sin(t * moveSpeed) * moveAmplitude;

        // 회전
        float rotZ = Mathf.Sin(t * rotateSpeed) * rotateAmplitude;

        transform.localPosition = initialPos + new Vector3(offsetX, 0, 0);
        transform.localRotation = initialRot * Quaternion.Euler(0, 0, rotZ);
    }
}

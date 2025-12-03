using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public bool enableMovement = true;  // false → 멈춤

    public float maxAngle = 30f;
    public float speed = 2f;

    private float baseZ;

    void Start()
    {
        baseZ = transform.localEulerAngles.z;
    }

    void Update()
    {
        if (!enableMovement)
            return;  // 🔥 움직임 완전히 제거됨

        float angle = Mathf.Sin(Time.time * speed) * maxAngle;
        transform.localRotation = Quaternion.Euler(0, 0, baseZ + angle);
    }
}

using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    [Header("Pendulum Settings")]
    public float maxAngle = 30f;   // 최대 흔들림 각도
    public float speed = 2f;       // 흔들리는 속도

    private float baseZ;

    void Start()
    {
        // 초기 각도 저장
        baseZ = transform.localEulerAngles.z;
    }

    void Update()
    {
        // -maxAngle ~ +maxAngle 범위의 사인파 각도
        float angle = Mathf.Sin(Time.time * speed) * maxAngle;

        // Z축 회전 적용 (윗점 고정 상태에서 아래가 진자 운동)
        transform.localRotation = Quaternion.Euler(
            0,
            0,
            baseZ + angle
        );
    }
}

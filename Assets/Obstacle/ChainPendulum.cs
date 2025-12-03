using UnityEngine;

public class ChainPendulum : MonoBehaviour
{
    [Header("Pendulum")]
    public bool active = false;   // ♥ 3장 이상일 때만 흔들기
    public float maxAngle = 25f;
    public float speed = 2.5f;

    private float baseZ;

    private void Start()
    {
        baseZ = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (!active) return;      // 오브젝트는 항상 켜져 있고,
                                  // active=true일 때만 흔듦
        float angle = Mathf.Sin(Time.time * speed) * maxAngle;
        transform.localRotation = Quaternion.Euler(0, 0, baseZ + angle);
    }

    public void SetActive(bool state)
    {
        active = state;

        // ✅ 더 이상 gameObject를 껐다 켰다 하지 않기
        // gameObject.SetActive(state);  <-- 이 줄 삭제!
    }
}

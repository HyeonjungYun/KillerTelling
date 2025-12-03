using UnityEngine;

public class ChainPendulum : MonoBehaviour
{
    [Header("Pendulum")]
    public bool active = false;       // ♥ 3장 이상일 때 활성화
    public float maxAngle = 25f;
    public float speed = 2.5f;

    private float baseZ;

    private void Start()
    {
        baseZ = transform.localEulerAngles.z;
    }

    private void Update()
    {
        if (!active) return;

        float angle = Mathf.Sin(Time.time * speed) * maxAngle;
        transform.localRotation = Quaternion.Euler(0, 0, baseZ + angle);
    }

    public void SetActive(bool state)
    {
        active = state;
        gameObject.SetActive(state);
    }
}

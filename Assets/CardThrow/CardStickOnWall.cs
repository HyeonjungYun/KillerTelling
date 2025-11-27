using UnityEngine;

public class CardStickOnWall : MonoBehaviour
{
    private bool stuck = false;
    private bool lerping = false;

    public float embedDepth = 0.04f;
    public float tiltAngle = 12f;
    public float stickLerpTime = 0.1f;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private float t = 0f;

    public void ForceStick(RaycastHit hit)
    {
        if (stuck) return;
        stuck = true;

        Vector3 normal = hit.normal;

        // 벽면 방향을 기준으로 카드가 '날아온' 방향대로 회전
        Quaternion baseRot = Quaternion.LookRotation(-normal, Vector3.up);

        Quaternion tilt = Quaternion.Euler(
            Random.Range(-tiltAngle, tiltAngle),
            Random.Range(-tiltAngle, tiltAngle),
            0f
        );

        targetRot = baseRot * tilt;
        targetPos = hit.point + (-normal * embedDepth);

        t = 0f;
        lerping = true;

        // Collider 비활성화
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // rigidbody 완전 정지
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    private void Update()
    {
        if (!lerping) return;

        t += Time.deltaTime / stickLerpTime;

        transform.position = Vector3.Lerp(transform.position, targetPos, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);

        if (t >= 1f)
            lerping = false;
    }
}
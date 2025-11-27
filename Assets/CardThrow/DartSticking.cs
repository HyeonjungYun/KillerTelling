using UnityEngine;

public class DartSticking : MonoBehaviour
{
    private bool stuck = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (stuck) return;

        // 벽 또는 과녁 태그 지정 필요
        if (col.collider.CompareTag("DartWall") || col.collider.CompareTag("Target"))
        {
            stuck = true;

            rb.isKinematic = true;
            rb.useGravity = false;

            // 충돌각에 맞춰서 회전 보정
            transform.rotation = Quaternion.LookRotation(col.contacts[0].normal * -1);

            Debug.Log("🎯 카드가 벽/과녁에 박힘!");
        }
    }
}

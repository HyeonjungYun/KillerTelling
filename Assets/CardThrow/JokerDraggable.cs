using UnityEngine;

public class JokerDraggable : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;

    private bool isDragging = false;
    private float dragZ = 2.5f;

    private Vector3 dragStartPos;
    private Vector3 dragStartMouse;

    private float throwForce = 15f;

    void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    //---------------------------------------
    // 클릭 시작
    //---------------------------------------
    private void OnMouseDown()
    {
        isDragging = true;

        dragStartPos = transform.position;
        dragStartMouse = Input.mousePosition;
    }

    //---------------------------------------
    // 드래그 중
    //---------------------------------------
    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Vector3 targetPos = ray.GetPoint(dragZ);

        transform.position = Vector3.Lerp(transform.position, targetPos, 0.4f);
    }

    //---------------------------------------
    // 클릭 떼면 → 던지기
    //---------------------------------------
    private void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        Vector3 dragEndMouse = Input.mousePosition;
        Vector3 dragDelta = dragEndMouse - dragStartMouse;

        // 좌우/상하 드래그 방향
        Vector3 sideDir =
            cam.transform.right * (dragDelta.x / Screen.width) +
            cam.transform.up * (dragDelta.y / Screen.height);

        // 기본 전방(카메라 방향)
        Vector3 forwardDir = cam.transform.forward;

        // 최종 방향 = 벽 방향 + 드래그 방향
        Vector3 worldDir = (forwardDir + sideDir).normalized;

        Debug.Log("카드 던짐! 방향 = " + worldDir);

        rb.AddForce(worldDir * throwForce, ForceMode.Impulse);
    }

    //---------------------------------------
    // 벽 충돌 → 박히기
    //---------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("BackWall"))
            return;

        // 물리 중단
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        // 충돌 지점 + 노멀
        Vector3 hitPoint = collision.contacts[0].point;
        Vector3 normal = collision.contacts[0].normal;

        // 살짝 앞으로 빼서 박히는 효과
        transform.position = hitPoint + normal * 0.02f;

        // 현재 회전 유지 (자연스러운 박힘)
        Quaternion currentRot = transform.rotation;

        // 완전 반대로 뒤집히는 것만 방지
        Vector3 forward = currentRot * Vector3.forward;
        if (Vector3.Dot(forward, -normal) < 0)
        {
            transform.rotation = Quaternion.LookRotation(-normal, transform.up);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class JokerDraggable : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;

    private bool dragging = false;
    private bool flying = false;

    private Vector3 lastMousePos;      // XZ 평면 기준 마우스 위치
    private Vector3 velocity;
    private float depth;

    public float throwPower = 15f;
    public float maxThrowSpeed = 25f;
    public float spinPower = 180f;
    public float castSkin = 0.02f;

    private Vector3 angularVel;

    public Transform backWall;
    public float wallStopOffset = 0.02f;

    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        boxCol = GetComponent<BoxCollider>();

        // 자동 연결
        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
        }
    }

    // -----------------------
    // 마우스 XY → 월드 XZ 평면 투영
    // -----------------------
    private Vector3 GetMouseOnXZPlane()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        // 현재 카드의 Y 높이와 평행한 XZ 평면
        float planeY = transform.position.y;

        float t = (planeY - ray.origin.y) / ray.direction.y;
        return ray.origin + ray.direction * t;
    }

    private void OnMouseDown()
    {
        dragging = true;
        flying = false;

        velocity = Vector3.zero;
        angularVel = Vector3.zero;

        lastMousePos = GetMouseOnXZPlane();

        Debug.Log("=== MouseDown ===");
        Debug.Log("lastMousePos : " + lastMousePos);
    }

    private void OnMouseDrag()
    {
        if (!dragging) return;

        Vector3 mouseXZ = GetMouseOnXZPlane();

        // XZ 평면 기준 이동 벡터
        velocity = (mouseXZ - lastMousePos) / Time.deltaTime;
        lastMousePos = mouseXZ;

        transform.position = mouseXZ;
    }

    private void OnMouseUp()
    {
        dragging = false;
        flying = true;

        Vector3 dragVel = velocity;

        // XZ 평면 속도 크기 계산
        float flatSpeed = new Vector2(dragVel.x, dragVel.z).magnitude;

        // 기존처럼 Y를 0으로 초기화하되,
        // "아주 약한" Y 성분만 다시 추가
        float upwardFactor = 0.1f; // ← 위/아래 민감도 (0.02 ~ 0.06 추천)
        dragVel.y = flatSpeed * upwardFactor;

        // Lerp로 과도한 튐 제거
        dragVel = Vector3.Lerp(Vector3.zero, dragVel, 0.65f);

        // 최종 속도 계산
        velocity = Vector3.ClampMagnitude(dragVel * throwPower, maxThrowSpeed);

        angularVel = Random.insideUnitSphere * spinPower;

        Debug.Log("=== OnMouseUp ===");
        Debug.Log("flatSpeed         : " + flatSpeed);
        Debug.Log("added Y           : " + dragVel.y);
        Debug.Log("final velocity    : " + velocity);
    }


    private void Update()
    {
        if (!flying) return;

        // BackWall null 체크 & 자동 재연결
        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
            if (backWall == null) return;
        }

        Vector3 move = velocity * Time.deltaTime;

        // Z축 강제 클램프 (뚫기 방지)
        float wallZ = backWall.position.z;
        if (transform.position.z >= wallZ - wallStopOffset)
        {
            flying = false;
            velocity = Vector3.zero;
            angularVel = Vector3.zero;

            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                wallZ - wallStopOffset
            );

            return;
        }

        // BoxCast 충돌 감지
        Vector3 halfSize = Vector3.Scale(boxCol.size * 0.5f, transform.localScale);
        if (Physics.BoxCast(
            transform.position,
            halfSize,
            velocity.normalized,
            out RaycastHit hit,
            transform.rotation,
            move.magnitude + castSkin))
        {
            if (hit.transform.CompareTag("BackWall"))
            {
                GetComponent<CardStickOnWall>().ForceStick(hit);
                flying = false;
                return;
            }
        }

        // 이동 + 회전
        transform.position += move;
        transform.Rotate(angularVel * Time.deltaTime, Space.Self);
    }
}

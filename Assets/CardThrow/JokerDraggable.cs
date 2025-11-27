using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class JokerDraggable : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;

    private bool dragging = false;
    private bool flying = false;

    private Vector3 lastMousePos;
    private Vector3 velocity;

    public float throwPower = 15f;
    public float maxThrowSpeed = 25f;
    public float spinPower = 180f;
    public float castSkin = 0.02f;

    private Vector3 angularVel;

    public Transform backWall;
    public float wallStopOffset = 0.02f;

    private float originalY;
    private float boostedY;
    private bool firstDragFrame = false;

    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        boxCol = GetComponent<BoxCollider>();

        originalY = transform.position.y;

        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
        }
    }

    private Vector3 GetMouseOnXZPlane(float y)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float t = (y - ray.origin.y) / ray.direction.y;
        return ray.origin + ray.direction * t;
    }

    private void OnMouseDown()
    {
        CameraElevator.Instance.RaiseCamera();

        dragging = true;
        flying = false;

        velocity = Vector3.zero;
        angularVel = Vector3.zero;

        // === 카메라 pitch 기반 Y 상승 예측 ===
        float pitch = cam.transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Abs(pitch);

        float camPitch01 = Mathf.Clamp01(pitch / 30f);
        boostedY = originalY + camPitch01 * 2.0f;

        // ❗ 절대 여기서 Y 이동시키지 않음 — 깜빡임 방지
        // transform.position = ...

        lastMousePos = GetMouseOnXZPlane(transform.position.y);
        firstDragFrame = true;

        Debug.Log($"[MouseDown] boostedY = {boostedY}");
    }

    private void OnMouseDrag()
    {
        if (!dragging) return;

        // === 드래그 첫 프레임에서 단 한 번만 Y 보정 ===
        if (firstDragFrame)
        {
            transform.position = new Vector3(
                transform.position.x,
                boostedY,
                transform.position.z
            );
            firstDragFrame = false;
        }

        Vector3 mouseXZ = GetMouseOnXZPlane(boostedY);

        velocity = (mouseXZ - lastMousePos) / Time.deltaTime;
        lastMousePos = mouseXZ;

        transform.position = mouseXZ;
    }

    private void OnMouseUp()
    {
        CameraElevator.Instance.ResetCamera();

        dragging = false;
        flying = true;

        Vector3 dragVel = velocity;

        float flatSpeed = new Vector2(dragVel.x, dragVel.z).magnitude;

        // 높이 증가 계산
        float upwardFactor = 0.1f;
        dragVel.y = flatSpeed * upwardFactor;

        dragVel = Vector3.Lerp(Vector3.zero, dragVel, 0.65f);

        velocity = Vector3.ClampMagnitude(dragVel * throwPower, maxThrowSpeed);

        angularVel = Random.insideUnitSphere * spinPower;

        Debug.Log("[MouseUp] Final Throw Velocity = " + velocity);
    }

    private void Update()
    {
        if (!flying) return;

        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
            if (backWall == null) return;
        }

        Vector3 move = velocity * Time.deltaTime;

        // === z 클램프 ===
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

        // === BoxCast 충돌 감지 ===
        Vector3 halfSize = Vector3.Scale(boxCol.size * 0.5f, transform.localScale);

        if (Physics.BoxCast(
            transform.position,
            halfSize,
            velocity.normalized,
            out RaycastHit hit,
            transform.rotation,
            move.magnitude + castSkin
        ))
        {
            if (hit.transform.CompareTag("BackWall"))
            {
                GetComponent<CardStickOnWall>().ForceStick(hit);
                flying = false;
                return;
            }
        }

        // === 이동 + 회전 ===
        transform.position += move;
        transform.Rotate(angularVel * Time.deltaTime, Space.Self);
    }
}

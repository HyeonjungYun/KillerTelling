using UnityEngine;

public class DartThrowCard : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private bool isDragging = false;

    private float throwForce = 25f;   // 던지는 힘
    private float airSpin = 400f;     // 회전 느낌

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            StartDrag();

        if (Input.GetMouseButtonUp(0))
            Release();

        if (isDragging)
            DragMove();
    }

    void StartDrag()
    {
        isDragging = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void DragMove()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5f;  // 카메라 앞쪽 심도
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        transform.position = Vector3.Lerp(transform.position, worldPos, 0.35f);
        transform.Rotate(Vector3.forward * airSpin * Time.deltaTime); // 회전
    }

    void Release()
    {
        isDragging = false;
        rb.useGravity = true;

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * 8f, ForceMode.Impulse);
    }
}

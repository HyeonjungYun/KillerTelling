using UnityEngine;

public class DartThrowCard : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private bool isDragging = false;

    private float throwForce = 25f;   // ������ ��
    private float airSpin = 400f;     // ȸ�� ����

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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void DragMove()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5f;  // ī�޶� ���� �ɵ�
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);

        transform.position = Vector3.Lerp(transform.position, worldPos, 0.35f);
        transform.Rotate(Vector3.forward * airSpin * Time.deltaTime); // ȸ��
    }

    void Release()
    {
        isDragging = false;
        rb.useGravity = true;

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * 8f, ForceMode.Impulse);
    }
}

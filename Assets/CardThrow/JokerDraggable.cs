using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    private CameraRotator camRotator;

    // ========= NEW: Trajectory ============
    private LineRenderer line;
    private Queue<Vector3> mouseSamples = new Queue<Vector3>();
    private float sampleDuration = 0.05f; // 최근 50ms만 사용

    public int trajectorySteps = 20;
    public float trajectoryStepDist = 0.3f;

    // Wall UI 카드 참조
    public WallCardPlacer wallPlacer;


    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        boxCol = GetComponent<BoxCollider>();
        originalY = transform.position.y;

        camRotator = FindFirstObjectByType<CameraRotator>();
        wallPlacer = FindObjectOfType<WallCardPlacer>();

        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
        }

        SetupLineRenderer();
    }

    //-------------------------------------------------
    // LineRenderer 준비
    //-------------------------------------------------
    private void SetupLineRenderer()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 0;
        line.enabled = false;

        line.material = new Material(Shader.Find("Sprites/Default"));
        line.material.hideFlags = HideFlags.DontSave | HideFlags.DontUnloadUnusedAsset;

        line.startColor = new Color(1f, 1f, 1f, 0.7f);
        line.endColor = new Color(1f, 1f, 1f, 0.2f);
        line.startWidth = 0.03f;
        line.endWidth = 0.01f;

        line.numCapVertices = 4;
        line.numCornerVertices = 4;
    }

    //-------------------------------------------------
    private Vector3 GetMouseOnXZPlane(float y)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        float t = (y - ray.origin.y) / ray.direction.y;
        return ray.origin + ray.direction * t;
    }

    //-------------------------------------------------
    private void OnMouseDown()
    {
        if (camRotator != null)
            camRotator.LookFront();

        dragging = true;
        flying = false;

        velocity = Vector3.zero;
        angularVel = Vector3.zero;

        float pitch = cam.transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Abs(pitch);

        float camPitch01 = Mathf.Clamp01(pitch / 30f);
        boostedY = originalY + camPitch01 * 2.0f;

        lastMousePos = GetMouseOnXZPlane(transform.position.y);
        firstDragFrame = true;

        mouseSamples.Clear();
        line.enabled = true;
    }

    //-------------------------------------------------
    private void OnMouseDrag()
    {
        if (!dragging) return;

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

        RecordMouseSample(mouseXZ);
        DrawTrajectory();
    }

    //-------------------------------------------------
    private void OnMouseUp()
    {
        if (camRotator != null)
            camRotator.LookDefault();

        dragging = false;
        flying = true;

        line.enabled = false;

        Vector3 dragDir = PredictDirection();
        Vector3 dragVel = dragDir * velocity.magnitude;

        float flatSpeed = new Vector2(dragVel.x, dragVel.z).magnitude;
        dragVel.y = flatSpeed * 0.1f;

        dragVel = Vector3.Lerp(Vector3.zero, dragVel, 0.65f);
        velocity = Vector3.ClampMagnitude(dragVel * throwPower, maxThrowSpeed);

        angularVel = Random.insideUnitSphere * spinPower;

        Debug.Log("[MouseUp] Final Throw Velocity = " + velocity);
    }

    //-------------------------------------------------
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

            TryHitUICard();
            return;
        }

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
                flying = false;

                GetComponent<CardStickOnWall>().ForceStick(hit);
                TryHitUICard();
                return;
            }
        }

        transform.position += move;
        transform.Rotate(angularVel * Time.deltaTime, Space.Self);
    }

    // ============================================================
    //   UI 과녁 카드 명중 판정
    // ============================================================
    private void TryHitUICard()
    {
        if (wallPlacer == null)
        {
            wallPlacer = FindObjectOfType<WallCardPlacer>();
            if (wallPlacer == null) return;
        }

        RectTransform targetArea = wallPlacer.targetArea;
        if (targetArea == null) return;

        float bestDistance = float.MaxValue;
        Image bestCard = null;

        Vector3 jokerPos = transform.position;

        for (int i = 0; i < targetArea.childCount; i++)
        {
            Transform c = targetArea.GetChild(i);

            if (!c.TryGetComponent<Image>(out Image img) || img.sprite == null)
                continue;

            float d = Vector3.Distance(jokerPos, c.position);

            if (d < bestDistance)
            {
                bestDistance = d;
                bestCard = img;
            }
        }

        if (bestCard == null || bestDistance > 0.35f)
            return;

        Debug.Log("🎯 UI 과녁 카드 명중: " + bestCard.sprite.name);

        HandManager.Instance.OnCardSelectedFromDeck(bestCard.sprite);

        Destroy(bestCard.gameObject);
    }

    // ============================================================
    //   마우스 샘플 업데이트
    // ============================================================
    private void RecordMouseSample(Vector3 pos)
    {
        mouseSamples.Enqueue(pos);

        while (mouseSamples.Count > 2)
            mouseSamples.Dequeue();
    }

    // ============================================================
    //   예측 방향 계산 (최근 50ms 기준)
    // ============================================================
    private Vector3 PredictDirection()
    {
        if (mouseSamples.Count < 2)
            return transform.forward;

        Vector3[] arr = mouseSamples.ToArray();
        return (arr[1] - arr[0]).normalized;
    }

    // ============================================================
    //   궤적 그리기
    // ============================================================
    private void DrawTrajectory()
    {
        if (!line.enabled) return;

        Vector3 dir = PredictDirection();
        List<Vector3> pts = new List<Vector3>();

        Vector3 pos = transform.position;
        for (int i = 0; i < trajectorySteps; i++)
        {
            pos += dir * trajectoryStepDist;
            pts.Add(pos);
        }

        line.positionCount = pts.Count;
        line.SetPositions(pts.ToArray());
    }
}

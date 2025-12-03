using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
public class JokerDraggable : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;

    public float throwPower = 25f;
    public float spinSpeed = 720f;
    public float aimSensitivity = 0.005f;

    public float baseCurvePower = 20f;
    public float scrollSensitivity = 5f;
    private float currentCurvePower;

    public enum TrajectoryType { Straight, CurveRight, CurveLeft }
    public TrajectoryType currentTrajectory = TrajectoryType.Straight;

    private enum State { Idle, MovingToHand, Selected, Aiming, Flying, Stuck }
    private State currentState = State.Idle;

    private LineRenderer lineRen;
    private Vector3 currentVelocity;
    private Vector3 currentAcceleration;

    private float startMouseX;
    private float startMouseY;

    public Transform backWall;
    public float wallStopOffset = 0.05f;

    public Transform handPos;
    private Vector3 fixedHandPos = new Vector3(0, 2, -4.5f);

    private CameraRotator camRotator;
    public WallCardPlacer wallPlacer;
    public float cameraReturnDelay = 0.8f;

    // 카메라 줌 컨트롤
    private CameraZoomToDart camZoom;

    // 이번 조커가 이미 카운트를 깎았는지 여부
    private bool jokerCountReduced = false;

    // ============================================================
    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        boxCol = GetComponent<BoxCollider>();
        AutoConfigureCollider();

        camRotator = FindFirstObjectByType<CameraRotator>();
        wallPlacer = FindFirstObjectByType<WallCardPlacer>();

        SetupLineRenderer();

        currentCurvePower = baseCurvePower;

        gameObject.layer = LayerMask.NameToLayer("Card");

        if (Camera.main != null)
            camZoom = Camera.main.GetComponent<CameraZoomToDart>();
    }

    private void AutoConfigureCollider()
    {
        boxCol.center = Vector3.zero;
        boxCol.size = new Vector3(1f, 1.4f, 0.05f);
        boxCol.isTrigger = false;
    }

    private void SetupLineRenderer()
    {
        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = 50;
        lineRen.enabled = false;

        Material m = new Material(Shader.Find("Sprites/Default"));
        m.renderQueue = 4000;
        m.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

        lineRen.material = m;
        lineRen.startWidth = 0.04f;
        lineRen.endWidth = 0.01f;
        lineRen.startColor = Color.white;
        lineRen.endColor = Color.white;
    }

    // ============================================================
    private void Update()
    {
        HandleMouseClick();
        HandleRightClick();
        HandleScroll();

        switch (currentState)
        {
            case State.MovingToHand: MoveToHand(); break;
            case State.Aiming: Aiming(); break;
                // Flying 은 FixedUpdate 에서만 처리
        }
    }

    // Flying 은 고정 시간 간격으로만 업데이트
    private void FixedUpdate()
    {
        if (currentState == State.Flying)
            Flying();
    }

    // ============================================================
    private void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 60f))
        {
            if (hit.collider.gameObject == this.gameObject)
                OnCardClicked();
        }
    }

    private void OnCardClicked()
    {
        if (camRotator)
            camRotator.LookFront();

        if (currentState == State.Idle)
        {
            ReduceJokerOnce();
            currentState = State.MovingToHand;
            return;
        }

        if (currentState == State.Selected)
        {
            currentState = State.Aiming;

            startMouseX = Input.mousePosition.x;
            startMouseY = Input.mousePosition.y;

            lineRen.enabled = true;

            transform.rotation = Quaternion.Euler(90, 0, 0);

            if (camZoom != null)
                camZoom.LockZoom();
        }
    }

    // ============================================================
    private void HandleRightClick()
    {
        if (currentState != State.Aiming && currentState != State.Selected) return;

        if (Input.GetMouseButtonDown(1))
        {
            currentTrajectory =
                (TrajectoryType)(((int)currentTrajectory + 1) % 3);

            currentCurvePower = baseCurvePower;
        }
    }

    private void HandleScroll()
    {
        if (currentState != State.Aiming) return;
        if (currentTrajectory == TrajectoryType.Straight) return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            currentCurvePower += scroll * scrollSensitivity;
            currentCurvePower = Mathf.Clamp(currentCurvePower, 0, 60);
        }
    }

    // ============================================================
    private void MoveToHand()
    {
        Vector3 target = handPos ? handPos.position : fixedHandPos;
        Quaternion targetRot = handPos ? handPos.rotation : Quaternion.Euler(60, 0, 0);

        transform.position = Vector3.MoveTowards(transform.position, target, 12f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 6f * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentState = State.Selected;

            if (camZoom != null)
                camZoom.UnlockZoom();
        }
    }

    // ============================================================
    private void Aiming()
    {
        if (Input.GetMouseButtonUp(0))
        {
            currentState = State.Flying;
            lineRen.enabled = false;

            Quaternion lookRot = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Euler(90, lookRot.eulerAngles.y, 0);

            if (camRotator) StartCoroutine(CameraDownDelay());
            return;
        }

        float dx = Input.mousePosition.x - startMouseX;
        float dy = Input.mousePosition.y - startMouseY;

        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();

        Vector3 aimDir =
            (camForward + camRight * dx * aimSensitivity + Vector3.up * dy * aimSensitivity)
            .normalized;

        Vector3 targetVelocity = aimDir * throwPower;

        Vector3 rightVec = cam.transform.right; rightVec.y = 0; rightVec.Normalize();

        switch (currentTrajectory)
        {
            case TrajectoryType.Straight: currentAcceleration = Vector3.zero; break;
            case TrajectoryType.CurveRight: currentAcceleration = rightVec * currentCurvePower; break;
            case TrajectoryType.CurveLeft: currentAcceleration = -rightVec * currentCurvePower; break;
        }

        float estTime = 10f / throwPower;
        currentVelocity = targetVelocity - currentAcceleration * estTime * 0.5f;

        DrawTrajectory(transform.position, currentVelocity, currentAcceleration);
    }

    // ============================================================
    private void DrawTrajectory(Vector3 pos, Vector3 vel, Vector3 acc)
    {
        float dt = Time.fixedDeltaTime;   // ← 실제 이동과 동일한 고정 시간 간격 사용
        Vector3 p = pos;
        Vector3 v = vel;

        for (int i = 0; i < lineRen.positionCount; i++)
        {
            lineRen.SetPosition(i, p);
            p += v * dt;
            v += acc * dt;
        }
    }

    // ============================================================
    private void Flying()
    {
        float dt = Time.fixedDeltaTime;   // ← DrawTrajectory와 동일한 dt

        Vector3 nextVel = currentVelocity + currentAcceleration * dt;
        Vector3 nextStep = nextVel * dt;

        Vector3 dir = nextVel.normalized;

        Vector3 castStart =
            transform.position +
            transform.up * 0.01f +
            dir * 0.02f;

        float castDist = nextStep.magnitude + 0.1f;

        int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");

        Vector3 halfExt = boxCol.size * 0.5f;
        halfExt.Scale(transform.localScale);

        if (Physics.BoxCast(
            castStart,
            halfExt,
            dir,
            out RaycastHit obstHit,
            transform.rotation,
            castDist,
            obstacleMask))
        {
            if (Vector3.Dot(dir, obstHit.point - castStart) > 0f)
            {
                transform.position = obstHit.point - dir * 0.02f;

                StartFalling(nextVel);
                return;
            }
        }

        int wallMask = 1 << LayerMask.NameToLayer("BackWallLayer");

        if (Physics.Raycast(transform.position, dir, out RaycastHit wallHit,
            castDist, wallMask))
        {
            transform.position = wallHit.point - dir * wallStopOffset;
            currentState = State.Stuck;

            if (camZoom != null)
                camZoom.UnlockZoom();

            TryHitUICard(wallHit.point);
            return;
        }

        currentVelocity = nextVel;
        transform.position += nextVel * dt;

        transform.Rotate(0, 0, spinSpeed * dt, Space.Self);
    }

    // ============================================================
    private IEnumerator DelayedFall()
    {
        yield return null;
        StartFalling(Vector3.zero);
    }

    private void StartFalling(Vector3 vel)
    {
        currentState = State.Stuck;
        spinSpeed = 0f;

        rb.isKinematic = false;
        rb.useGravity = true;

        Physics.gravity = new Vector3(0, -4f, 0);

        Vector3 downward = Vector3.down * 0.8f;
        Vector3 smallSide = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0,
            Random.Range(-0.2f, 0.2f)
        );

        rb.linearVelocity = downward + smallSide;

        rb.angularVelocity = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        );

        if (camZoom != null)
            camZoom.UnlockZoom();

        Debug.Log("💥 장애물 충돌 → 자연스러운 낙하 시작");
    }

    // ============================================================
    private void TryHitUICard(Vector3 hitPos)
    {
        if (!wallPlacer || !wallPlacer.targetArea) return;

        float bestDist = float.MaxValue;
        Image best = null;

        foreach (Transform child in wallPlacer.targetArea)
        {
            if (!child.TryGetComponent(out Image img)) continue;
            if (!img.sprite) continue;

            float d = Vector3.Distance(hitPos, child.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = img;
            }
        }

        if (best == null || bestDist > 0.2f) return;

        HandManager.Instance.OnCardHitByThrow(best.sprite);
        Destroy(best.gameObject);
    }

    private IEnumerator CameraDownDelay()
    {
        yield return new WaitForSeconds(cameraReturnDelay);
        if (camRotator)
            camRotator.LookDefault();
    }

    // ============================================================
    private void ReduceJokerOnce()
    {
        if (jokerCountReduced) return;
        jokerCountReduced = true;

        if (JokerStack3D.Instance != null)
        {
            JokerStack3D.Instance.ReduceCountOnly();
        }
        else
        {
            Debug.LogWarning("JokerDraggable: JokerStack3D.Instance 가 없습니다.");
        }
    }
}

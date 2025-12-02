using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
public class JokerDraggable : MonoBehaviour
{
    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;
    private CameraZoomToDart camZoom;

    [Header("Throw Settings")]
    public float throwPower = 25f;
    public float maxThrowSpeed = 25f;
    public float spinSpeed = 720f;
    public float aimSensitivity = 0.005f;

    [Header("Curve Settings")]
    public float baseCurvePower = 20f;
    public float scrollSensitivity = 5f;
    private float currentCurvePower;

    public enum TrajectoryType { Straight, CurveRight, CurveLeft }
    [Header("State Info")]
    public TrajectoryType currentTrajectory = TrajectoryType.Straight;

    public float spinPower = 180f;
    public float castSkin = 0.02f;
    public Transform backWall;
    public float wallStopOffset = 0.02f;
    public float curvePower = 0f;

    private LineRenderer lineRen;

    private Vector3 currentVelocity;
    private Vector3 currentAcceleration;

    public Transform handPosition;
    private Vector3 fixedHandPos = new Vector3(0f, 2.0f, -5.5f);

    private float startMouseX;
    private float startMouseY;

    private enum State { Idle, MovingToHand, Selected, Aiming, Flying, Stuck }
    private State currentState = State.Idle;

    private CameraRotator camRotator;

    private Queue<Vector3> mouseSamples = new Queue<Vector3>();
    private float sampleDuration = 0.05f;
    public int trajectorySteps = 20;
    public float trajectoryStepDist = 0.3f;

    public WallCardPlacer wallPlacer;

    private void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        boxCol = GetComponent<BoxCollider>();

        lineRen = GetComponent<LineRenderer>();
        lineRen.positionCount = 50;
        lineRen.enabled = false;
        lineRen.startWidth = 0.05f;
        lineRen.endWidth = 0.05f;

        Shader shader = Shader.Find("Sprites/Default");
        Material lineMat = new Material(shader);
        lineMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        lineMat.renderQueue = 4000;
        lineRen.material = lineMat;
        lineRen.startColor = Color.white;
        lineRen.endColor = Color.white;

        camRotator = FindFirstObjectByType<CameraRotator>();
        wallPlacer = FindObjectOfType<WallCardPlacer>();

        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
        }

        currentCurvePower = baseCurvePower;
        camZoom = Camera.main.GetComponent<CameraZoomToDart>();
    }

    private void OnMouseDown()
    {
        // 첫 클릭: 스택 → 손 위치로 이동
        if (currentState == State.Idle)
        {
            Debug.Log("🟦 Joker: Idle → MovingToHand");
            currentState = State.MovingToHand;
        }
        // 손에 들고 있는 상태에서 다시 클릭: 조준 시작 + 줌 잠금
        else if (currentState == State.Selected)
        {
            Debug.Log("🟩 Joker: Selected → Aiming");
            currentState = State.Aiming;
            lineRen.enabled = true;

            startMouseX = Input.mousePosition.x;
            startMouseY = Input.mousePosition.y;

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // 조준 시작 시에만 줌 잠금
            if (camZoom != null)
                camZoom.LockZoom();
        }
    }

    private void Update()
    {
        HandleRightClickSwitch();
        HandleMouseScroll();

        switch (currentState)
        {
            case State.MovingToHand:
                MoveToHandLogic();
                break;
            case State.Aiming:
                AimingLogic();
                break;
            case State.Flying:
                FlyingLogic();
                break;
        }
    }

    private void HandleRightClickSwitch()
    {
        if ((currentState == State.Selected || currentState == State.Aiming) &&
            Input.GetMouseButtonDown(1))
        {
            int nextType = ((int)currentTrajectory + 1) % 3;
            currentTrajectory = (TrajectoryType)nextType;
            currentCurvePower = baseCurvePower;

            Debug.Log($"궤적 변경: {currentTrajectory}");
        }
    }

    private void HandleMouseScroll()
    {
        if (currentState != State.Aiming || currentTrajectory == TrajectoryType.Straight) return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentCurvePower += scroll * scrollSensitivity;
            currentCurvePower = Mathf.Clamp(currentCurvePower, 0f, 60f);
        }
    }

    private void MoveToHandLogic()
    {
        Vector3 targetPos = (handPosition != null) ? handPosition.position : fixedHandPos;
        Quaternion targetRot = (handPosition != null) ? handPosition.rotation : Quaternion.Euler(60f, 0f, 0f);

        float step = maxThrowSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, step * 0.5f);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            transform.position = targetPos;
            transform.rotation = targetRot;
            currentState = State.Selected;
            Debug.Log("🟨 Joker: MovingToHand → Selected");
            // ❗ 여기서는 줌 잠그지 않음
        }
    }

    private void AimingLogic()
    {
        // 마우스 떼면 던지기 시작
        if (Input.GetMouseButtonUp(0))
        {
            lineRen.enabled = false;
            currentState = State.Flying;
            Debug.Log("🟥 Joker: Aiming → Flying");
            return;
        }

        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();

        float deltaX = Input.mousePosition.x - startMouseX;
        float deltaY = Input.mousePosition.y - startMouseY;

        Vector3 aimDirection =
            (camForward +
            (camRight * deltaX * aimSensitivity) +
            (Vector3.up * deltaY * aimSensitivity)).normalized;

        Vector3 targetVelocity = aimDirection * throwPower;

        currentAcceleration = Vector3.zero;
        Vector3 rightVec = cam.transform.right;
        rightVec.y = 0; rightVec.Normalize();

        switch (currentTrajectory)
        {
            case TrajectoryType.Straight:
                currentAcceleration = Vector3.zero;
                break;
            case TrajectoryType.CurveRight:
                currentAcceleration = rightVec * currentCurvePower;
                break;
            case TrajectoryType.CurveLeft:
                currentAcceleration = -rightVec * currentCurvePower;
                break;
        }

        float estimatedTime = 10f / throwPower;

        currentVelocity = targetVelocity - (currentAcceleration * estimatedTime * 0.5f);

        DrawTrajectoryPath(transform.position, currentVelocity, currentAcceleration);

        if (currentVelocity != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Euler(90f, lookRot.eulerAngles.y, 0f);
        }
    }

    private void DrawTrajectoryPath(Vector3 startPos, Vector3 startVel, Vector3 accel)
    {
        Vector3 simPos = startPos;
        Vector3 simVel = startVel;
        float timeStep = 0.02f;

        lineRen.positionCount = 50;

        for (int i = 0; i < lineRen.positionCount; i++)
        {
            lineRen.SetPosition(i, simPos);
            simPos += simVel * timeStep;
            simVel += accel * timeStep;
        }
    }

    private void FlyingLogic()
    {
        float dt = Time.deltaTime;

        Vector3 nextVelocity = currentVelocity + (currentAcceleration * dt);
        Vector3 nextStep = nextVelocity * dt;

        if (Physics.Raycast(transform.position,
                            nextVelocity.normalized,
                            out RaycastHit hit,
                            nextStep.magnitude + 0.1f))
        {
            if (hit.collider.CompareTag("BackWall") || (backWall && hit.transform == backWall))
            {
                transform.position = hit.point - (nextVelocity.normalized * wallStopOffset);
                SendMessage("ForceStick", hit, SendMessageOptions.DontRequireReceiver);
                currentState = State.Stuck;

                Debug.Log("🧱 Joker: 벽에 박힘 → Stuck");

                // 🔓 다시 R/E 줌 허용
                if (camZoom != null)
                    camZoom.UnlockZoom();

                return;
            }
        }

        currentVelocity += currentAcceleration * dt;
        transform.position += currentVelocity * dt;

        transform.Rotate(0, 0, spinSpeed * dt, Space.Self);
    }
}

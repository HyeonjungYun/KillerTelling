using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
public class JokerDraggable : MonoBehaviour
{
    // === [기존 변수] ===
    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;

    [Header("Throw Settings")]
    public float throwPower = 25f;
    public float maxThrowSpeed = 25f;
    public float spinSpeed = 720f;
    public float aimSensitivity = 0.005f;

    [Header("Curve Settings")]
    public float baseCurvePower = 20f; // [변경] 기본 커브 강도 (Inspector 설정값)
    public float scrollSensitivity = 5f; // [신규] 휠 한 칸당 변하는 커브 양

    // 현재 적용 중인 커브 강도 (휠로 조절됨)
    private float currentCurvePower;

    // 궤적 타입
    public enum TrajectoryType { Straight, CurveRight, CurveLeft }

    [Header("State Info")]
    public TrajectoryType currentTrajectory = TrajectoryType.Straight;

    // 더미 변수
    public float spinPower = 180f; // (구 throwPower 대신 경고 방지용 남김)
    public float castSkin = 0.02f;
    public Transform backWall;
    public float wallStopOffset = 0.02f;
    public float curvePower = 0f; // (구 변수, inspector 호환용 더미)

    private LineRenderer lineRen;

    private Vector3 currentVelocity;
    private Vector3 currentAcceleration;

    public Transform handPosition;
    private Vector3 fixedHandPos = new Vector3(0f, 2.0f, -5.5f);

    private float startMouseX;
    private float startMouseY;

    private enum State { Idle, MovingToHand, Selected, Aiming, Flying, Stuck }
    private State currentState = State.Idle;

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

        // [핵심 수정] 무조건 최상단에 그리기 위한 재질 설정
        // 기존 재질이 있든 없든, 강제로 최상단 렌더링 속성을 덮어씌웁니다.

        // 1. 쉐이더 가져오기 (가볍고 범용적인 스프라이트 쉐이더 사용)
        Shader shader = Shader.Find("Sprites/Default");
        Material lineMat = new Material(shader);

        // 2. ZTest (깊이 검사) 끄기
        // UnityEngine.Rendering.CompareFunction.Always = 8
        // "내 앞에 벽이 있든 말든 무조건 그린다"
        lineMat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);

        // 3. RenderQueue (그리는 순서) 최상위로 올리기
        // Geometry(2000) -> Transparent(3000) -> Overlay(4000)
        // "모든 3D 물체를 다 그린 다음, 맨 마지막에 붓칠을 한다"
        lineMat.renderQueue = 4000;

        // 생성한 재질 적용
        lineRen.material = lineMat;

        // 색상 설정 (혹시 핑크색으로 보일까봐 안전장치)
        lineRen.startColor = Color.white; // 원하는 색으로 변경 가능 (예: Color.red)
        lineRen.endColor = Color.white;


        if (backWall == null)
        {
            GameObject wall = GameObject.FindWithTag("BackWall");
            if (wall != null) backWall = wall.transform;
        }

        // 초기값 설정
        currentCurvePower = baseCurvePower;
    }

    private void OnMouseDown()
    {
        if (currentState == State.Idle)
        {
            currentState = State.MovingToHand;
        }
        else if (currentState == State.Selected)
        {
            currentState = State.Aiming;
            lineRen.enabled = true;

            startMouseX = Input.mousePosition.x;
            startMouseY = Input.mousePosition.y;

            // 조준 시작할 때마다 기본값으로 리셋할지, 유지할지 선택 가능.
            // 여기서는 유지하는 방식으로 둠.

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private void Update()
    {
        // 1. 우클릭: 궤적 타입 변경
        HandleRightClickSwitch();

        // 2. [신규] 휠 스크롤: 궤적 세기 조절
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
        if ((currentState == State.Selected || currentState == State.Aiming) && Input.GetMouseButtonDown(1))
        {
            // 궤적 순환
            int nextType = ((int)currentTrajectory + 1) % 3;
            currentTrajectory = (TrajectoryType)nextType;

            // 궤적 타입을 바꿀 때마다 커브 강도를 기본값으로 리셋 (사용자 편의)
            currentCurvePower = baseCurvePower;

            Debug.Log($"궤적 변경: {currentTrajectory}");
        }
    }

    // === [신규 기능] 마우스 휠 로직 ===
    private void HandleMouseScroll()
    {
        // 조준 중이 아니거나, 일직선(Straight) 모드라면 휠 무시 (조건 1)
        if (currentState != State.Aiming || currentTrajectory == TrajectoryType.Straight) return;

        float scroll = Input.mouseScrollDelta.y;

        // 스크롤 입력이 있을 때만 계산
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 휠 올리면(+), 내리면(-)
            currentCurvePower += scroll * scrollSensitivity;

            // (조건 2) 커브 강도는 0 이상으로만 유지 (절대 음수가 되지 않게 Clamp)
            // 최대치(Max)는 60 정도로 제한하여 너무 심하게 꺾이는 것 방지
            currentCurvePower = Mathf.Clamp(currentCurvePower, 0f, 60f);

            // 디버깅용 (필요 없으면 주석)
            // Debug.Log($"Curve Power: {currentCurvePower}");
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
        }
    }

    private void AimingLogic()
    {
        if (Input.GetMouseButtonUp(0))
        {
            lineRen.enabled = false;
            currentState = State.Flying;
            return;
        }

        // 1. [기존] 순수한 직선 방향(목표지점) 계산
        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();

        float deltaX = Input.mousePosition.x - startMouseX;
        float deltaY = Input.mousePosition.y - startMouseY;

        Vector3 aimDirection = (camForward + (camRight * deltaX * aimSensitivity) + (Vector3.up * deltaY * aimSensitivity)).normalized;

        // 이것이 우리가 '도달하고 싶은' 목표 속도입니다.
        Vector3 targetVelocity = aimDirection * throwPower;

        // 2. [기존] 가속도(a) 계산 (커브 힘)
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

        // === [핵심 수정] 속도 보정 (Compensated Velocity) ===
        // 목표 지점에 도달하기 위해, 가속도의 반대 방향으로 초기 속도를 틀어줍니다.

        // 예상 비행 시간 (거리 / 속력) -> 대략 10m 앞 기준
        // (정확하지 않아도 궤적 시각화에는 충분합니다)
        float estimatedTime = 10f / throwPower;

        // 보정 공식: V_real = V_target - (0.5 * a * t)
        // 가속도가 오른쪽으로 작용하면, 던질 때는 왼쪽으로 던져야 중앙에 맞음
        currentVelocity = targetVelocity - (currentAcceleration * estimatedTime * 0.5f);

        // 3. 궤적 그리기
        DrawTrajectoryPath(transform.position, currentVelocity, currentAcceleration);

        // 4. [시각적 디테일] 카드의 회전도 실제 날아가는 방향(보정된 방향)을 보게 함
        // 이렇게 해야 카드가 처음엔 옆을 보고 있다가 점점 중앙으로 휘어들어가는 느낌이 남
        if (currentVelocity != Vector3.zero)
        {
            // 90도 눕힌 상태에서 Y축 회전(Yaw)만 날아가는 방향에 맞춤
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

        if (Physics.Raycast(transform.position, nextVelocity.normalized, out RaycastHit hit, nextStep.magnitude + 0.1f))
        {
            if (hit.collider.CompareTag("BackWall") || (backWall && hit.transform == backWall))
            {
                transform.position = hit.point - (nextVelocity.normalized * wallStopOffset);
                SendMessage("ForceStick", hit, SendMessageOptions.DontRequireReceiver);
                currentState = State.Stuck;
                return;
            }
        }

        currentVelocity += currentAcceleration * dt;
        transform.position += currentVelocity * dt;

        transform.Rotate(0, 0, spinSpeed * dt, Space.Self);
    }
}
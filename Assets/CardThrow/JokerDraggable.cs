using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
public class JokerDraggable : MonoBehaviour
{
    public static JokerDraggable ActiveJoker = null;

    private Camera cam;
    private Rigidbody rb;
    private BoxCollider boxCol;

    [Header("Throw Settings")]
    public float throwPower = 25f;
    public float aimSensitivity = 0.005f;
    public float spinSpeed = 720f;

    [Header("Curve Settings")]
    public float baseCurvePower = 20f;
    public float scrollSensitivity = 5f;
    private float currentCurvePower;

    [Header("Energy Drink Effect")]
    public float shakeIntensity = 10f; // 흔들림 강도
    public float shakeSpeed = 3f;     // 흔들림 속도

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

    private CameraZoomToDart camZoom;
    private bool jokerCountReduced = false;

    [Header("Sound")]
    public AudioClip pickSound;       // Idle → MovingToHand
    public AudioClip readySound;      // 손 도착
    public AudioClip aimSound;        // Selected → Aiming
    public AudioClip throwSound;      // 던질 때
    public AudioClip hitWallSound;    // 벽 충돌
    public AudioClip hitObstacleSound;// 장애물 충돌
    public AudioClip fallSound;       // 낙하 시작
    public AudioClip hitTargetSound;  // UI 카드 맞추기

    private AudioSource audioSource;


    private void Awake()
    {

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;


        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        boxCol = GetComponent<BoxCollider>();
        boxCol.center = Vector3.zero;
        boxCol.size = new Vector3(1f, 1.4f, 0.05f);
        boxCol.isTrigger = false;

        camRotator = FindFirstObjectByType<CameraRotator>();
        wallPlacer = FindFirstObjectByType<WallCardPlacer>();

        SetupLineRenderer();
        currentCurvePower = baseCurvePower;
        gameObject.layer = LayerMask.NameToLayer("Card");

        if (Camera.main != null)
            camZoom = Camera.main.GetComponent<CameraZoomToDart>();
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

    private void Update()
    {
        // 🔥 [핵심] 연출 중(입력 차단 상태)이면 카드 조작 불가
        if (CardGraveyardManager.IsInputBlocked) return;

        HandleMouseClick();
        HandleRightClick();
        HandleScroll();

        switch (currentState)
        {
            case State.MovingToHand: MoveToHand(); break;
            case State.Aiming: Aiming(); break;
        }
    }

    private void FixedUpdate()
    {
        if (currentState == State.Flying)
            Flying();
    }

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
        if (ActiveJoker != null && ActiveJoker != this) return;

        if (camRotator) camRotator.LookFront();

        if (currentState == State.Idle)
        {
            ActiveJoker = this;
            currentState = State.MovingToHand;
            if (pickSound) audioSource.PlayOneShot(pickSound);
            return;
        }

        if (currentState == State.Selected)
        {
            currentState = State.Aiming;
            if (aimSound) audioSource.PlayOneShot(aimSound);
            startMouseX = Input.mousePosition.x;
            startMouseY = Input.mousePosition.y;
            lineRen.enabled = true;
            transform.rotation = Quaternion.Euler(90, 0, 0);

            if (camZoom != null) camZoom.LockZoom();
        }
    }

    private void HandleRightClick()
    {
        if (currentState != State.Aiming && currentState != State.Selected) return;
        if (Input.GetMouseButtonDown(1))
        {
            currentTrajectory = (TrajectoryType)(((int)currentTrajectory + 1) % 3);
            currentCurvePower = baseCurvePower;
        }
    }

    private void HandleScroll()
    {
        if (currentState != State.Aiming || currentTrajectory == TrajectoryType.Straight) return;
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            currentCurvePower += scroll * scrollSensitivity;
            currentCurvePower = Mathf.Clamp(currentCurvePower, 0, 60);
        }
    }

    private void MoveToHand()
    {
        Vector3 target = handPos ? handPos.position : fixedHandPos;
        Quaternion targetRot = handPos ? handPos.rotation : Quaternion.Euler(60, 0, 0);

        transform.position = Vector3.MoveTowards(transform.position, target, 12f * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 6f * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentState = State.Selected;
            if (readySound) audioSource.PlayOneShot(readySound);
            if (HandManager.Instance != null) HandManager.Instance.SetExchangeMode(true);
            if (camZoom != null) camZoom.UnlockZoom();
        }
    }

    private void Aiming()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ReduceJokerOnce();
            currentState = State.Flying;
            if (throwSound) audioSource.PlayOneShot(throwSound);
            lineRen.enabled = false;
            Quaternion lookRot = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Euler(90, lookRot.eulerAngles.y, 0);
            if (camRotator) StartCoroutine(CameraDownDelay());
            return;
        }

        float dx = Input.mousePosition.x - startMouseX;
        float dy = Input.mousePosition.y - startMouseY;

        // 🔥 다이아 4장 이상이면 수전증(떨림) 효과 적용
        if (CheckDiamondCondition())
        {
            float noiseX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeIntensity;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeIntensity;
            dx += noiseX;
            dy += noiseY;
        }

        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();
        Vector3 aimDir = (camForward + camRight * dx * aimSensitivity + Vector3.up * dy * aimSensitivity).normalized;

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

    // 🔥 무덤에서 다이아 카드 개수 체크
    private bool CheckDiamondCondition()
    {
        if (CardGraveyardManager.Instance == null) return false;
        List<Sprite> graveyard = CardGraveyardManager.Instance.StoredSprites;
        if (graveyard == null) return false;

        int diamondCount = 0;
        foreach (Sprite spr in graveyard)
        {
            if (spr == null) continue;
            if (char.ToUpper(spr.name[spr.name.Length - 1]) == 'D') diamondCount++;
        }
        return diamondCount >= 4;
    }

    private void DrawTrajectory(Vector3 pos, Vector3 vel, Vector3 acc)
    {
        float dt = Time.fixedDeltaTime;
        Vector3 p = pos;
        Vector3 v = vel;
        for (int i = 0; i < lineRen.positionCount; i++)
        {
            lineRen.SetPosition(i, p);
            p += v * dt;
            v += acc * dt;
        }
    }

    private void Flying()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 nextVel = currentVelocity + currentAcceleration * dt;
        Vector3 nextStep = nextVel * dt;
        Vector3 dir = nextVel.normalized;
        Vector3 castStart = transform.position + transform.up * 0.01f + dir * 0.02f;
        float castDist = nextStep.magnitude + 0.1f;

        int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
        Vector3 halfExt = boxCol.size * 0.5f; halfExt.Scale(transform.localScale);

        if (Physics.BoxCast(castStart, halfExt, dir, out RaycastHit obstHit, transform.rotation, castDist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Dot(dir, obstHit.point - castStart) > 0f)
            {
                transform.position = obstHit.point - dir * 0.02f;
                if (hitObstacleSound) audioSource.PlayOneShot(hitObstacleSound);
                StartFalling(nextVel);
                return;
            }
        }

        int wallMask = 1 << LayerMask.NameToLayer("BackWallLayer");
        if (Physics.Raycast(castStart, dir, out RaycastHit wallHit, castDist, wallMask, QueryTriggerInteraction.Ignore))
        {
            if (wallHit.collider.CompareTag("BackWall"))
            {
                transform.position = wallHit.point - dir * wallStopOffset;
                currentState = State.Stuck;
                ClearActiveIfSelf();
                if (hitWallSound) audioSource.PlayOneShot(hitWallSound);
                if (camZoom != null) camZoom.UnlockZoom();
                TryHitUICard(wallHit.point);
                return;
            }
        }

        currentVelocity = nextVel;
        transform.position += nextVel * dt;
        transform.Rotate(0, 0, spinSpeed * dt, Space.Self);
    }

    private void StartFalling(Vector3 vel)
    {
        if (fallSound) audioSource.PlayOneShot(fallSound);
        currentState = State.Stuck;
        spinSpeed = 0f;
        ClearActiveIfSelf();
        rb.isKinematic = false;
        rb.useGravity = true;
        Physics.gravity = new Vector3(0, -4f, 0);
        rb.linearVelocity = Vector3.down * 0.8f + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.2f, 0.2f));
        rb.angularVelocity = Random.insideUnitSphere;
        if (camZoom != null) camZoom.UnlockZoom();
    }

    private void TryHitUICard(Vector3 hitPos)
    {

        if (!wallPlacer || !wallPlacer.targetArea) return;
        Camera uiCam = Camera.main;
        Vector2 screenPoint = uiCam.WorldToScreenPoint(hitPos);
        Image best = null; float bestDist = float.MaxValue;

        foreach (Transform child in wallPlacer.targetArea)
        {
            string nm = child.name;
            if (nm.Contains("Back") || nm.Contains("Board") || nm.Contains("Dart") || nm.Contains("Background")) continue;
            if (!child.TryGetComponent(out Image img) || !img.sprite) continue;
            if (!RectTransformUtility.RectangleContainsScreenPoint(child as RectTransform, screenPoint, uiCam)) continue;

            float d = Vector3.Distance(hitPos, child.position);
            if (d < bestDist) { bestDist = d; best = img; }
        }

        if (best != null)
        {
            if (hitTargetSound) audioSource.PlayOneShot(hitTargetSound);
            HandManager.Instance.OnCardHitByThrow(best.sprite);
            Destroy(best.gameObject);
        }
    }

    private IEnumerator CameraDownDelay()
    {
        yield return new WaitForSeconds(cameraReturnDelay);
        if (camRotator) camRotator.LookDefault();
    }

    private void ReduceJokerOnce()
    {
        if (jokerCountReduced) return;
        jokerCountReduced = true;
        if (JokerStack3D.Instance != null) JokerStack3D.Instance.ReduceCountOnly();
    }

    private void ClearActiveIfSelf() { if (ActiveJoker == this) ActiveJoker = null; }
    public static void DestroyActiveJokerImmediately() { if (ActiveJoker != null) { Destroy(ActiveJoker.gameObject); ActiveJoker = null; } }


}
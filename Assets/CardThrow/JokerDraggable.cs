using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
[RequireComponent(typeof(LineRenderer))]
public class JokerDraggable : MonoBehaviour
{
    public static JokerDraggable ActiveJoker = null;

    // 🔥 튜토리얼 페이즈에서 조커를 몇 번 "실제로 집었는지" 추적
    private static int tutorialPickCount = 0;

    public static void ResetTutorialPickCount()
    {
        tutorialPickCount = 0;
    }

    // ✅ 이 조커가 이미 "집었다" 이벤트를 튜토에 알렸는지(중복 방지)
    private bool notifiedTutorialPick = false;

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
    public float shakeIntensity = 10f;
    public float shakeSpeed = 3f;

    public enum TrajectoryType { Straight, CurveRight, CurveLeft }
    public TrajectoryType currentTrajectory = TrajectoryType.Straight;

    private enum State { Idle, MovingToHand, Selected, Aiming, Flying, Stuck }
    private State currentState = State.Idle;

    private LineRenderer lineRen;
    private Vector3 currentVelocity;
    private Vector3 currentAcceleration;

    private float startMouseX;
    private float startMouseY;

    [Header("Board / Wall")]
    public Transform backWall;
    public float wallStopOffset = 0.05f;

    [Header("Hand Position")]
    public Transform handPos;
    private Vector3 fixedHandPos = new Vector3(0, 2, -4.5f);

    private CameraRotator camRotator;
    public WallCardPlacer wallPlacer;
    public float cameraReturnDelay = 0.8f;

    private CameraZoomToDart camZoom;
    private bool jokerCountReduced = false;

    // =========================================================
    // ✅ 추가: 소멸 이펙트 설정(우측 상단 덱에서 카드 가져올 때 사용)
    // =========================================================
    [Header("Vanish Effect (Deck Pick)")]
    [Tooltip("조커 소멸 연출 시간")]
    public float vanishDuration = 0.25f;

    [Tooltip("소멸 시 최종 스케일 배율(0이면 완전 축소)")]
    public float vanishEndScaleMultiplier = 0.0f;

    private bool isVanishing = false;
    private Coroutine vanishRoutine;

    // ✅ 렌더러/머티리얼 캐시(페이드용)
    private Renderer[] cachedRenderers;
    private readonly List<Material> cachedMats = new List<Material>();

    // =========================================================
    // 🔊 (추가) SFX / BGM
    // =========================================================
    [Header("Sound (SFX)")]
    public AudioClip pickSound;        // Idle → MovingToHand
    public AudioClip readySound;       // 손 도착
    public AudioClip aimSound;         // Selected → Aiming
    public AudioClip throwSound;       // 던질 때
    public AudioClip hitWallSound;     // 벽 충돌
    public AudioClip hitObstacleSound; // 장애물 충돌
    public AudioClip fallSound;        // 낙하 시작
    public AudioClip hitTargetSound;   // UI 카드 맞추기
    public AudioClip consumeSound;     // 소멸 연출(덱 교환 소모 등)

    [Header("Sound (BGM / Loop)")]
    [Tooltip("조준 상태(Aiming)에서 루프로 재생할 배경음(없으면 무시)")]
    public AudioClip aimingLoopBgm;

    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    private bool IsStage1TutorialNow()
    {
        return StageManager.Instance != null
            && StageManager.Instance.currentStage == 1
            && StageManager.Instance.IsStage1TutorialPhase;
    }

    private void OnEnable()
    {
        // 튜토 페이즈에서 값 이상치 보정
        if (IsStage1TutorialNow())
        {
            if (tutorialPickCount < 0 || tutorialPickCount > 1000)
                tutorialPickCount = 0;
        }

        notifiedTutorialPick = false;
    }

    private void Awake()
    {
        // 🔊 (추가) 오디오 소스 2개 구성: SFX / BGM(루프)
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

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

        CacheFadeMaterials();
    }

    // 🔊 (추가) 공용 재생 함수들
    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip);
    }

    private void StartAimingBgm()
    {
        if (bgmSource == null) return;
        if (aimingLoopBgm == null) return;

        if (bgmSource.isPlaying && bgmSource.clip == aimingLoopBgm) return;

        bgmSource.clip = aimingLoopBgm;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    private void StopAimingBgm()
    {
        if (bgmSource == null) return;
        if (!bgmSource.isPlaying) return;
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    private void CacheFadeMaterials()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedMats.Clear();

        if (cachedRenderers == null) return;

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            var r = cachedRenderers[i];
            if (r == null) continue;

            // ✅ material 접근 시 인스턴스 머티리얼이 생성되어 개별 페이드 가능
            var m = r.material;
            if (m == null) continue;

            cachedMats.Add(m);
        }
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
        if (CardGraveyardManager.IsInputBlocked) return;
        if (isVanishing) return; // ✅ 소멸 중엔 입력/로직 막기

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
        if (isVanishing) return;
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

        // Idle → 손으로 이동
        if (currentState == State.Idle)
        {
            ActiveJoker = this;
            currentState = State.MovingToHand;

            // 🔊 pick SFX
            PlaySfx(pickSound);
            return;
        }

        // Selected → 조준 모드
        if (currentState == State.Selected)
        {
            currentState = State.Aiming;

            // 🔊 aim SFX + 조준 BGM 시작
            PlaySfx(aimSound);
            StartAimingBgm();

            startMouseX = Input.mousePosition.x;
            startMouseY = Input.mousePosition.y;

            lineRen.enabled = true;
            transform.rotation = Quaternion.Euler(90, 0, 0);

            if (camZoom != null)
                camZoom.LockZoom();
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
        if (currentState != State.Aiming) return;
        if (currentTrajectory == TrajectoryType.Straight) return;

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

            // 🔊 ready SFX
            PlaySfx(readySound);

            if (HandManager.Instance != null)
                HandManager.Instance.SetExchangeMode(true);

            if (camZoom != null)
                camZoom.UnlockZoom();

            // ✅ “진짜로 손에 들렸을 때”만 1회 튜토 이벤트 발생
            if (!notifiedTutorialPick && IsStage1TutorialNow() && TutorialManager.Instance != null)
            {
                notifiedTutorialPick = true;
                tutorialPickCount++;

                if (tutorialPickCount == 1)
                    TutorialManager.Instance.OnJokerPicked();
                else if (tutorialPickCount == 2)
                    TutorialManager.Instance.OnSecondJokerPicked();
            }
        }
    }

    private void Aiming()
    {
        if (Input.GetMouseButtonUp(0))
        {
            ReduceJokerOnce();
            ClearActiveIfSelf();

            currentState = State.Flying;
            lineRen.enabled = false;

            // 🔊 throw SFX + 조준 BGM 종료
            PlaySfx(throwSound);
            StopAimingBgm();

            Quaternion lookRot = Quaternion.LookRotation(currentVelocity);
            transform.rotation = Quaternion.Euler(90, lookRot.eulerAngles.y, 0);

            if (IsStage1TutorialNow() && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnJokerThrown();
            }

            if (camRotator)
                StartCoroutine(CameraDownDelay());
            return;
        }

        float dx = Input.mousePosition.x - startMouseX;
        float dy = Input.mousePosition.y - startMouseY;

        if (CheckDiamondCondition())
        {
            float noiseX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f) * shakeIntensity;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f) * shakeIntensity;
            dx += noiseX;
            dy += noiseY;
        }

        Vector3 camForward = cam.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cam.transform.right; camRight.y = 0; camRight.Normalize();

        Vector3 aimDir =
            (camForward + camRight * dx * aimSensitivity + Vector3.up * dy * aimSensitivity)
            .normalized;

        Vector3 targetVelocity = aimDir * throwPower;

        Vector3 rightVec = cam.transform.right; rightVec.y = 0; rightVec.Normalize();

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

        float estTime = 10f / throwPower;
        currentVelocity = targetVelocity - currentAcceleration * estTime * 0.5f;

        DrawTrajectory(transform.position, currentVelocity, currentAcceleration);
    }

    private bool CheckDiamondCondition()
    {
        if (CardGraveyardManager.Instance == null) return false;

        List<Sprite> graveyard = CardGraveyardManager.Instance.StoredSprites;
        if (graveyard == null) return false;

        int diamondCount = 0;
        foreach (Sprite spr in graveyard)
        {
            if (spr == null) continue;
            char last = spr.name[spr.name.Length - 1];
            if (char.ToUpper(last) == 'D')
                diamondCount++;
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

        Vector3 castStart =
            transform.position +
            transform.up * 0.01f +
            dir * 0.02f;

        float castDist = nextStep.magnitude + 0.1f;

        int obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
        Vector3 halfExt = boxCol.size * 0.5f; halfExt.Scale(transform.localScale);

        if (Physics.BoxCast(castStart, halfExt, dir,
                out RaycastHit obstHit, transform.rotation,
                castDist, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Dot(dir, obstHit.point - castStart) > 0f)
            {
                transform.position = obstHit.point - dir * 0.02f;

                // 🔊 obstacle hit SFX
                PlaySfx(hitObstacleSound);

                StartFalling(nextVel);
                return;
            }
        }

        int wallMask = 1 << LayerMask.NameToLayer("BackWallLayer");

        if (Physics.Raycast(castStart, dir,
                out RaycastHit wallHit, castDist,
                wallMask, QueryTriggerInteraction.Ignore))
        {
            if (wallHit.collider.CompareTag("BackWall"))
            {
                transform.position = wallHit.point - dir * wallStopOffset;
                currentState = State.Stuck;

                Transform parent = wallHit.collider.transform;
                if (wallPlacer != null && wallPlacer.targetArea != null)
                    parent = wallPlacer.targetArea;

                transform.SetParent(parent, true);

                ClearActiveIfSelf();

                // 🔊 wall hit SFX
                PlaySfx(hitWallSound);

                if (camZoom != null)
                    camZoom.UnlockZoom();

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
        currentState = State.Stuck;
        spinSpeed = 0f;

        ClearActiveIfSelf();

        // 🔊 fall SFX
        PlaySfx(fallSound);

        rb.isKinematic = false;
        rb.useGravity = true;

        Physics.gravity = new Vector3(0, -4f, 0);

        rb.linearVelocity =
            Vector3.down * 0.8f +
            new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.2f, 0.2f));

        rb.angularVelocity = Random.insideUnitSphere;

        if (camZoom != null)
            camZoom.UnlockZoom();
    }

    private void TryHitUICard(Vector3 hitPos)
    {
        if (!wallPlacer || !wallPlacer.targetArea) return;

        Camera uiCam = Camera.main;
        Vector2 screenPoint = uiCam.WorldToScreenPoint(hitPos);

        Image best = null;
        float bestDist = float.MaxValue;

        foreach (Transform child in wallPlacer.targetArea)
        {
            string nm = child.name;
            if (nm.Contains("Back") ||
                nm.Contains("back") ||
                nm.Contains("Board") ||
                nm.Contains("Dart") ||
                nm.Contains("Background"))
                continue;

            if (!child.TryGetComponent(out Image img)) continue;
            if (!img.sprite) continue;

            RectTransform rt = child as RectTransform;
            if (rt == null) continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, uiCam))
                continue;

            float d = Vector3.Distance(hitPos, child.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = img;
            }
        }

        if (best == null) return;

        // 🔊 target hit SFX
        PlaySfx(hitTargetSound);

        if (HandManager.Instance != null)
            HandManager.Instance.OnCardHitByThrow(best.sprite);

        Destroy(best.gameObject);
    }

    private IEnumerator CameraDownDelay()
    {
        yield return new WaitForSeconds(cameraReturnDelay);

        if (camRotator)
            camRotator.LookDefault();

        // ✅ 카메라 복귀 이벤트만 전달
        if (StageManager.Instance != null &&
            StageManager.Instance.currentStage == 1 &&
            StageManager.Instance.IsStage1TutorialPhase &&
            TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnCameraBackToTable();
        }
    }

    private void ReduceJokerOnce()
    {
        if (jokerCountReduced) return;
        jokerCountReduced = true;

        if (JokerStack3D.Instance != null)
            JokerStack3D.Instance.ReduceCountOnly();
    }

    private void ClearActiveIfSelf()
    {
        if (ActiveJoker == this)
            ActiveJoker = null;
    }

    // =========================================================
    // ✅ 추가: “덱 교환 소모”용 외부 호출 API
    // =========================================================
    public void PlayConsumeEffectAndDestroy()
    {
        // 🔊 consume SFX
        PlaySfx(consumeSound);

        VanishAndDestroy(vanishDuration);
    }

    // =========================================================
    // ✅ 추가: “페이드아웃 + 축소” 후 제거 (Renderer 기반 페이드)
    // =========================================================
    private IEnumerator VanishAndDestroyRoutine(float duration)
    {
        if (isVanishing) yield break;
        isVanishing = true;

        // 🔊 혹시 조준 중 소멸되면 BGM 정리
        StopAimingBgm();

        // 충돌/입력/물리 제거
        if (boxCol != null) boxCol.enabled = false;
        if (lineRen != null) lineRen.enabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 상태 잠금
        currentState = State.Stuck;

        // ActiveJoker 정리
        ClearActiveIfSelf();

        // 머티리얼 캐시가 비어있으면 다시 캐시
        if (cachedMats.Count == 0) CacheFadeMaterials();

        // 시작 알파(가능하면 _Color 기준)
        float startAlpha = 1f;
        if (cachedMats.Count > 0 && cachedMats[0] != null && cachedMats[0].HasProperty("_Color"))
            startAlpha = cachedMats[0].color.a;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * vanishEndScaleMultiplier;

        float dur = Mathf.Max(0.05f, duration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            // scale
            transform.localScale = Vector3.Lerp(startScale, endScale, u);

            // fade (모든 Renderer material의 _Color.a)
            float a = Mathf.Lerp(startAlpha, 0f, u);
            for (int i = 0; i < cachedMats.Count; i++)
            {
                var m = cachedMats[i];
                if (m == null) continue;
                if (!m.HasProperty("_Color")) continue;

                Color c = m.color;
                c.a = a;
                m.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public void VanishAndDestroy(float duration = -1f)
    {
        if (isVanishing) return;

        float d = (duration > 0f) ? duration : vanishDuration;

        if (vanishRoutine != null)
        {
            StopCoroutine(vanishRoutine);
            vanishRoutine = null;
        }

        vanishRoutine = StartCoroutine(VanishAndDestroyRoutine(d));
    }

    public static void DestroyActiveJokerImmediately()
    {
        if (ActiveJoker != null)
        {
            ActiveJoker.VanishAndDestroy();
            ActiveJoker = null;
        }
    }

    public static void VanishActiveJoker(float duration = 0.25f)
    {
        if (ActiveJoker != null)
        {
            ActiveJoker.VanishAndDestroy(duration);
            ActiveJoker = null;
        }
    }

    private void OnDisable()
    {
        // 🔊 오브젝트 비활성/파괴 시 BGM 정리(안전)
        StopAimingBgm();
    }

    private void OnDestroy()
    {
        // 🔊 파괴 시 BGM 정리(안전)
        StopAimingBgm();
    }
}

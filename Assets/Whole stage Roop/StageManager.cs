using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Stage Settings")]
    [Tooltip("현재 진행중인 실제 스테이지 번호 (1~maxStage)")]
    public int currentStage = 1;
    public int maxStage = 4;

    [Header("Camera Anchors")]
    public Transform camPosRadio;
    public Transform camPosGame;   // ✅ 기본 게임 시점 (줌 아웃 위치로 사용)
    public Transform camPosZoomIn; // ✅ [추가됨] 줌 인 했을 때의 목표 앵커

    [Header("UI Control")]
    public List<GameObject> gameUIGroup;

    [Header("References")]
    public JokerStack3D jokerStack;
    public GoalDeckManager goalDeckManager;
    public GameStartCardPicker startCardPicker;
    public HandManager handManager;

    [Header("Dialogues")]
    public List<StageDialogueProfile> stageDialogues;

    [Header("UI References")]
    public GameObject mainMenuCanvas;

    // (호환용) 기존 변수명 유지: continueButton을 CheckButton으로 쓰는 경우 대비
    public GameObject continueButton;

    [Header("Result UI (Optional)")]
    public GameObject resultCanvasRoot;   // ResultCanvas 루트(부모)
    public GameObject resultMainButton;   // 실패/클리어 상관없이 표시(Main)
    public GameObject resultCheckButton;  // 클리어 시에만 표시(Check)

    [Header("Obstacle References (SFX Control)")]
    public MovingTargetObstacle movingTarget;
    public ChainPendulum chainPendulum;

    [Header("UI / Flow SFX")]
    public AudioClip gameStartClickSFX;      // 메인 버튼 클릭
    [Header("Dialogue / Transition SFX")]
    public AudioClip radioStaticSFX;     // 치지직
    public AudioClip screenTransitionSFX; // 화면전환


    [Range(0f, 1f)]
    public float uiSfxVolume = 1f;

    private AudioSource uiAudioSource;

    private bool isStageEnded = false;

    // ✅ Stage1 "튜토리얼 페이즈" 여부
    [SerializeField] private bool isStage1TutorialPhase = false;
    public bool IsStage1TutorialPhase => isStage1TutorialPhase;

    // ✅ 튜토리얼 클리어 후 "Check 눌렀을 때 Stage1 시작"을 위한 pending
    private bool pendingTutorialClear = false;
    public bool HasPendingTutorialClear => pendingTutorialClear;

    // ✅ (추가) "실제 스테이지 클리어 후" Check 눌렀을 때 post/다음 스테이지 진행
    private bool pendingStageClear = false;
    private int pendingStageIndex = -1;
    public bool HasPendingStageClear => pendingStageClear;

    // ✅ [추가됨] 줌 상태 관리 변수
    private bool isZoomedIn = false;

    private Coroutine showResultRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 🔊 UI SFX AudioSource
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
        uiAudioSource.volume = uiSfxVolume;

        EnsureResultButtonsRef();
        HideResultButtons();
    }

    // ✅ [추가됨] Update 함수에서 R키 입력을 감지
    private void Update()
    {
        // 게임 UI가 켜져 있을 때만(게임 진행 중일 때만) 줌 기능 작동
        // 혹은 스테이지가 끝났으면 줌 안되게 막으려면 if (isStageEnded) return; 추가
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleZoom();
        }
    }

    // ✅ [추가됨] 줌 토글 기능 (앵커 위치로 즉시 이동)
    public void ToggleZoom()
    {
        if (camPosGame == null || camPosZoomIn == null)
        {
            Debug.LogWarning("Camera Anchors are missing!");
            return;
        }

        isZoomedIn = !isZoomedIn; // 상태 반전
        Transform targetAnchor = isZoomedIn ? camPosZoomIn : camPosGame;

        // 해결책: 직접 옮기는 대신 CameraDirector에게 "이제 여기가 목표야"라고 알려줌
        if (CameraDirector.Instance != null)
        {
            // CameraDirector가 알아서 이동시키도록 위임
            CameraDirector.Instance.MoveToTarget(targetAnchor);

            // 만약 CameraDirector가 부드럽게 이동(Lerp)하는데, 
            // 줌인만큼은 '순간이동' 하고 싶다면 아래 코드를 추가해보세요 (CameraDirector 구조에 따라 다름)
            // Camera.main.transform.position = targetAnchor.position;
            // Camera.main.transform.rotation = targetAnchor.rotation;
        }
        else
        {
            // CameraDirector가 없을 때만 직접 이동
            if (Camera.main != null)
            {
                Camera.main.transform.position = targetAnchor.position;
                Camera.main.transform.rotation = targetAnchor.rotation;
            }
        }
    }

    // ... (이하 기존 코드와 동일) ...

    private void PlayUISfx(AudioClip clip)
    {
        if (clip == null || uiAudioSource == null) return;
        uiAudioSource.volume = uiSfxVolume;
        uiAudioSource.PlayOneShot(clip);
    }

    private void Start()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayMainMenu();

        if (jokerStack == null) jokerStack = FindFirstObjectByType<JokerStack3D>();
        if (goalDeckManager == null) goalDeckManager = FindFirstObjectByType<GoalDeckManager>();
        if (startCardPicker == null) startCardPicker = FindFirstObjectByType<GameStartCardPicker>();
        if (handManager == null) handManager = FindFirstObjectByType<HandManager>();

        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            OnClickGameStart();
        }
    }

    private void StartDialogueWithRadio(List<DialogueLine> dialogue, System.Action onDialogueEnd)
    {
        PlayUISfx(radioStaticSFX);
        DialogueManager.Instance.StartDialogue(dialogue, onDialogueEnd);
    }

    public void OnClickGameStart()
    {
        PlayUISfx(gameStartClickSFX);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        if (BGMManager.Instance != null)
            BGMManager.Instance.Stop();

        currentStage = 1;
        isStage1TutorialPhase = false;

        pendingTutorialClear = false;
        pendingStageClear = false;
        pendingStageIndex = -1;

        isStageEnded = false;
        HideResultButtons();

        // 게임 시작 시 줌 상태 초기화
        isZoomedIn = false;

        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        StageDialogueProfile stage1Profile = GetDialogueProfile(1);

        if (DialogueManager.Instance != null &&
            stage1Profile != null &&
            stage1Profile.preStageDialogue != null &&
            stage1Profile.preStageDialogue.Count > 0)
        {
            StartDialogueWithRadio(
                stage1Profile.preStageDialogue,
                () =>
                {
                    PlayUISfx(screenTransitionSFX);
                    StartStage1TutorialPhase();
                }
            );
        }
        else
        {
            PlayUISfx(screenTransitionSFX);
            StartStage1TutorialPhase();
        }
    }

    private void StartStage1TutorialPhase()
    {
        currentStage = 1;
        isStage1TutorialPhase = true;

        pendingTutorialClear = false;
        pendingStageClear = false;
        pendingStageIndex = -1;

        isStageEnded = false;
        HideResultButtons();

        // 스테이지 시작 시 줌 상태 초기화
        isZoomedIn = false;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayTutorial();

        if (CameraDirector.Instance != null && camPosGame != null)
            CameraDirector.Instance.MoveToTarget(camPosGame);

        SetGameUIActive(true);

        if (DeckManager.Instance != null)
            DeckManager.Instance.ResetDeckForNewStage();

        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        if (handManager != null) handManager.ClearSelectedCards3D();
        if (jokerStack != null) jokerStack.OnStageStart();

        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(0);
        if (startCardPicker != null) startCardPicker.SetupForStage(0);

        JokerDraggable.ResetTutorialPickCount();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.StartTutorial();
        else
            Debug.LogWarning("⚠ TutorialManager.Instance 없음 → 튜토 텍스트가 안 뜹니다.");
    }

    public void ConfirmTutorialClearAndStartStage1()
    {
        if (!pendingTutorialClear) return;
        pendingTutorialClear = false;
        ResetFromTutorialToMain();
        StartRealStage1Gameplay();
    }

    private void StartRealStage1Gameplay()
    {
        currentStage = 1;
        isStage1TutorialPhase = false;
        pendingStageClear = false;
        pendingStageIndex = -1;
        isStageEnded = false;
        HideResultButtons();

        // 줌 초기화
        isZoomedIn = false;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(1);

        if (CameraDirector.Instance != null && camPosGame != null)
            CameraDirector.Instance.MoveToTarget(camPosGame);

        SetGameUIActive(true);

        if (DeckManager.Instance != null)
            DeckManager.Instance.ResetDeckForNewStage();

        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        if (handManager != null) handManager.ClearSelectedCards3D();
        if (jokerStack != null) jokerStack.OnStageStart();

        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(1);
        if (startCardPicker != null) startCardPicker.SetupForStage(1);
    }

    private void StartRealStageGameplay(int stageIndex)
    {
        currentStage = Mathf.Clamp(stageIndex, 1, maxStage);
        isStage1TutorialPhase = false;
        pendingStageClear = false;
        pendingStageIndex = -1;
        isStageEnded = false;
        HideResultButtons();

        // 줌 초기화
        isZoomedIn = false;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(currentStage);

        if (CameraDirector.Instance != null && camPosGame != null)
            CameraDirector.Instance.MoveToTarget(camPosGame);

        SetGameUIActive(true);

        if (movingTarget != null) movingTarget.ResumeLoopSFX();
        if (chainPendulum != null) chainPendulum.ResumeLoopSFX();

        if (DeckManager.Instance != null)
            DeckManager.Instance.ResetDeckForNewStage();

        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        if (handManager != null) handManager.ClearSelectedCards3D();
        if (jokerStack != null) jokerStack.OnStageStart();

        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(currentStage);
        if (startCardPicker != null) startCardPicker.SetupForStage(currentStage);
    }

    public void OnSubmitResult(bool isClear)
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.Stop();

        if (movingTarget != null) movingTarget.StopLoopSFX();
        if (chainPendulum != null) chainPendulum.StopLoopSFX();

        if (currentStage == 1 && isStage1TutorialPhase && isClear)
        {
            pendingTutorialClear = true;
            return;
        }

        if (!isClear) return;

        pendingStageClear = true;
        pendingStageIndex = currentStage;
    }

    public void ConfirmStageClearAndProceed()
    {
        if (!pendingStageClear) return;

        int clearedStage = pendingStageIndex;
        pendingStageClear = false;
        pendingStageIndex = -1;

        PlayPostDialogueAndEnterNextStage(clearedStage);
    }

    private void PlayPostDialogueAndEnterNextStage(int clearedStage)
    {
        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        StageDialogueProfile profile = GetDialogueProfile(clearedStage);

        if (DialogueManager.Instance != null &&
            profile != null &&
            profile.postStageDialogue != null &&
            profile.postStageDialogue.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(profile.postStageDialogue, () => GoToNextStageFrom(clearedStage));
        }
        else
        {
            GoToNextStageFrom(clearedStage);
        }
    }

    private void GoToNextStageFrom(int clearedStage)
    {
        int nextStage = clearedStage + 1;

        if (nextStage > maxStage)
        {
            if (BGMManager.Instance != null)
                BGMManager.Instance.PlayEnding();

            Debug.Log("🏆 모든 스테이지 클리어!");
            return;
        }

        EnterStageSequence(nextStage);
    }

    public void GoToNextStage()
    {
        if (currentStage >= maxStage)
        {
            if (BGMManager.Instance != null)
                BGMManager.Instance.PlayEnding();

            Debug.Log("🏆 모든 스테이지 클리어!");
            return;
        }

        EnterStageSequence(currentStage + 1);
    }

    public void EnterStageSequence(int stageIndex)
    {
        int next = Mathf.Clamp(stageIndex, 1, maxStage);

        isStageEnded = false;
        HideResultButtons();
        pendingStageClear = false;
        pendingStageIndex = -1;

        // 줌 초기화
        isZoomedIn = false;

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(next);

        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        StageDialogueProfile profile = GetDialogueProfile(next);

        if (DialogueManager.Instance != null &&
            profile != null &&
            profile.preStageDialogue != null &&
            profile.preStageDialogue.Count > 0)
        {
            StartDialogueWithRadio(
                profile.preStageDialogue,
                () =>
                {
                    PlayUISfx(screenTransitionSFX);
                    StartRealStageGameplay(next);
                }
            );
        }
        else
        {
            PlayUISfx(screenTransitionSFX);
            StartRealStageGameplay(next);
        }
    }

    private void ResetFromTutorialToMain()
    {
        Debug.Log("🧹 Stage1 튜토 종료 → Stage1 실제 시작 전 리셋");

        if (jokerStack != null) jokerStack.ResetForNewGame(7);
        else if (JokerStack3D.Instance != null) JokerStack3D.Instance.ResetForNewGame(7);

        if (CardGraveyardManager.Instance != null)
            CardGraveyardManager.Instance.ClearAll();

        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        foreach (var jd in FindObjectsOfType<JokerDraggable>())
            Destroy(jd.gameObject);

        if (handManager != null) handManager.ClearSelectedCards3D();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ForceHide();

        JokerDraggable.ResetTutorialPickCount();
    }

    private void SetGameUIActive(bool isActive)
    {
        if (gameUIGroup == null) return;
        foreach (var ui in gameUIGroup)
            if (ui != null) ui.SetActive(isActive);
    }

    private StageDialogueProfile GetDialogueProfile(int stage)
    {
        if (stageDialogues == null) return null;
        return stageDialogues.Find(x => x.stageIndex == stage);
    }

    public void OnStageEnd(bool isClear)
    {
        if (isStageEnded) return;
        isStageEnded = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (showResultRoutine != null)
        {
            StopCoroutine(showResultRoutine);
            showResultRoutine = null;
        }

        showResultRoutine = StartCoroutine(ShowResultButtonsNextFrame(isClear));
    }

    public void OnStageEnd()
    {
        OnStageEnd(false);
    }

    private IEnumerator ShowResultButtonsNextFrame(bool isClear)
    {
        EnsureResultButtonsRef();

        if (resultCanvasRoot != null)
            resultCanvasRoot.SetActive(true);

        yield return null;
        yield return new WaitForEndOfFrame();

        EnsureResultButtonsRef();

        if (resultMainButton != null) resultMainButton.SetActive(true);
        if (resultCheckButton != null) resultCheckButton.SetActive(isClear);

        if (continueButton != null)
            continueButton.SetActive(isClear);

        showResultRoutine = null;
    }

    public void OnClickContinue()
    {
        GoToNextStage();
    }

    private void EnsureResultButtonsRef()
    {
        if (resultCanvasRoot == null)
        {
            var rc = GameObject.Find("ResultCanvas");
            if (rc != null) resultCanvasRoot = rc;
        }

        if (resultMainButton == null)
        {
            var go = GameObject.Find("GoHomeButton");
            if (go != null) resultMainButton = go;
        }

        if (resultCheckButton == null)
        {
            var go = GameObject.Find("CheckButton");
            if (go != null) resultCheckButton = go;
        }

        if (continueButton == null && resultCheckButton != null)
            continueButton = resultCheckButton;
    }

    private void HideResultButtons()
    {
        EnsureResultButtonsRef();

        if (showResultRoutine != null)
        {
            StopCoroutine(showResultRoutine);
            showResultRoutine = null;
        }

        if (resultMainButton != null) resultMainButton.SetActive(false);
        if (resultCheckButton != null) resultCheckButton.SetActive(false);
        if (continueButton != null) continueButton.SetActive(false);
    }

    // ================================================================
    // ✅ 메인 메뉴로 돌아가기 (홈 버튼 기능)
    // ================================================================
    public void OnClickReturnToMain()
    {
        Debug.Log("🏠 메인 메뉴로 복귀");

        // 1. 다른 UI들 끄기 (예시: 인게임 UI, 일시정지 창, 상점 등)
        if (resultCanvasRoot != null) resultCanvasRoot.SetActive(false);

        // 2. 메인 메뉴 Canvas 켜기
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }

        // 1. 사운드 정리 (배경음 & 효과음)
        PlayUISfx(gameStartClickSFX); // 클릭 소리 (선택 사항)

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayMainMenu(); // 메인 브금으로 변경

        // 장애물 반복 효과음 끄기
        if (movingTarget != null) movingTarget.StopLoopSFX();
        if (chainPendulum != null) chainPendulum.StopLoopSFX();

        // 2. 코루틴 및 진행 상태 초기화
        StopAllCoroutines(); // 결과창 연출 등이 돌고 있었다면 강제 중단
        isStageEnded = false;
        pendingStageClear = false;
        pendingTutorialClear = false;
        pendingStageIndex = -1;

        // 3. 줌 상태 초기화 (중요: 줌인 상태로 나가면 꼬일 수 있음)
        isZoomedIn = false;

        // 4. 카메라 이동 (라디오 위치로)
        if (CameraDirector.Instance != null && camPosRadio != null)
        {
            CameraDirector.Instance.MoveToTarget(camPosRadio);
        }

        // 5. 게임 오브젝트 데이터 초기화 (카드, 조커 등 싹 다 지우기)
        ResetGameElements();

        // 6. UI 전환 (게임 UI 끄고, 메인 메뉴 켜기)
        HideResultButtons(); // 결과 버튼들 숨기기
        SetGameUIActive(false);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
        }

        // 7. 마우스 커서 정리
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 내부적으로 게임 요소들을 싹 청소하는 헬퍼 함수
    private void ResetGameElements()
    {
        // 조커 스택 초기화
        if (jokerStack != null) jokerStack.ResetForNewGame(7);
        else if (JokerStack3D.Instance != null) JokerStack3D.Instance.ResetForNewGame(7);

        // 무덤 초기화
        if (CardGraveyardManager.Instance != null)
            CardGraveyardManager.Instance.ClearAll();

        // 시작 카드 선택기 초기화
        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        // 화면에 떠있는 드래그 중인 조커들 삭제
        foreach (var jd in FindObjectsOfType<JokerDraggable>())
        {
            if (jd != null) Destroy(jd.gameObject);
        }

        // 손패 초기화
        if (handManager != null) handManager.ClearSelectedCards3D();

        // 튜토리얼 UI 숨기기
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ForceHide();

        JokerDraggable.ResetTutorialPickCount();
    }
}
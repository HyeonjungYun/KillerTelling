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
    public Transform camPosGame;

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

    private Coroutine showResultRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        EnsureResultButtonsRef();
        HideResultButtons();
    }

    private void Start()
    {
        // 🎵 BGM: 메인 메뉴 브금 (mainMenuCanvas가 있는 구조라면 Start에서 깔아주는게 가장 안전)
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

    public void OnClickGameStart()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);

        currentStage = 1;
        isStage1TutorialPhase = false;

        pendingTutorialClear = false;
        pendingStageClear = false;
        pendingStageIndex = -1;

        isStageEnded = false;

        HideResultButtons();

        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        // 🎵 BGM: 게임 시작 시 1스테이지 테마로 전환 (참고 코드와 동일한 핵심)
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(1);

        StageDialogueProfile stage1Profile = GetDialogueProfile(1);

        if (DialogueManager.Instance != null &&
            stage1Profile != null &&
            stage1Profile.preStageDialogue != null &&
            stage1Profile.preStageDialogue.Count > 0)
        {
            DialogueManager.Instance.StartDialogue(stage1Profile.preStageDialogue, StartStage1TutorialPhase);
        }
        else
        {
            StartStage1TutorialPhase();
        }
    }

    // ✅ Stage1 튜토 페이즈 세팅
    private void StartStage1TutorialPhase()
    {
        currentStage = 1;
        isStage1TutorialPhase = true;

        pendingTutorialClear = false;
        pendingStageClear = false;
        pendingStageIndex = -1;

        isStageEnded = false;

        HideResultButtons();

        // 🎵 BGM: 튜토리얼도 Stage1 테마를 쓰는 구조라면 여기서 한번 더 보장
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

        // 튜토 목표/시작카드
        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(0);
        if (startCardPicker != null) startCardPicker.SetupForStage(0);

        JokerDraggable.ResetTutorialPickCount();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.StartTutorial();
        else
            Debug.LogWarning("⚠ TutorialManager.Instance 없음 → 튜토 텍스트가 안 뜹니다.");
    }

    // ✅ 튜토리얼 결과창에서 Check 눌렀을 때 호출될 공개 메서드
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

        // 🎵 BGM: 실전 Stage1 시작 보장
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

        // 실제 Stage1 목표/시작카드
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

        // 🎵 BGM: 해당 스테이지 테마로 전환
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(currentStage);

        if (CameraDirector.Instance != null && camPosGame != null)
            CameraDirector.Instance.MoveToTarget(camPosGame);

        SetGameUIActive(true);

        if (DeckManager.Instance != null)
            DeckManager.Instance.ResetDeckForNewStage();

        if (startCardPicker != null)
            startCardPicker.ClearTargetCards();

        if (handManager != null) handManager.ClearSelectedCards3D();
        if (jokerStack != null) jokerStack.OnStageStart();

        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(currentStage);
        if (startCardPicker != null) startCardPicker.SetupForStage(currentStage);
    }

    // ================================================================
    // ✅ 제출 결과 처리
    //  - 튜토리얼 클리어: pendingTutorialClear만 세팅 (결과창은 SubmitButton이 띄움)
    //  - 실제 스테이지 클리어: pendingStageClear만 세팅 (post는 Check 클릭 시 실행)
    // ================================================================
    public void OnSubmitResult(bool isClear)
    {
        // 튜토리얼 클리어 -> Check 클릭하면 Stage1 실제 시작
        if (currentStage == 1 && isStage1TutorialPhase && isClear)
        {
            pendingTutorialClear = true;
            return;
        }

        // 실패는 SubmitButton이 결과창(FAIL) 처리만 하므로 여기선 할 일 없음
        if (!isClear)
        {
            return;
        }

        // 실제 스테이지 클리어 -> Check 클릭하면 post 대사(있으면) -> 다음 스테이지
        pendingStageClear = true;
        pendingStageIndex = currentStage;
    }

    // ✅ 실제 스테이지 클리어 결과창에서 Check 눌렀을 때 호출
    public void ConfirmStageClearAndProceed()
    {
        if (!pendingStageClear) return;

        int clearedStage = pendingStageIndex;
        pendingStageClear = false;
        pendingStageIndex = -1;

        // 클리어한 스테이지가 1이면 Stage1 post 대사 -> Stage2
        // 클리어한 스테이지가 2면 Stage2 post -> Stage3 ...
        PlayPostDialogueAndEnterNextStage(clearedStage);
    }

    private void PlayPostDialogueAndEnterNextStage(int clearedStage)
    {
        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        SetGameUIActive(false);

        StageDialogueProfile profile = GetDialogueProfile(clearedStage);

        // post 대사 있으면 끝난 뒤 다음 스테이지 시퀀스
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
            // 🎵 BGM: 엔딩 브금
            if (BGMManager.Instance != null)
                BGMManager.Instance.PlayEnding();

            Debug.Log("🏆 모든 스테이지 클리어!");
            return;
        }

        EnterStageSequence(nextStage);
    }

    // (외부에서 호출할 수 있도록 기존 메서드 유지)
    public void GoToNextStage()
    {
        if (currentStage >= maxStage)
        {
            // 🎵 BGM: 엔딩 브금
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

        // 🎵 BGM: 다음 스테이지 테마로 미리 전환 (대사도 그 스테이지 분위기로)
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
            DialogueManager.Instance.StartDialogue(profile.preStageDialogue, () => StartRealStageGameplay(next));
        }
        else
        {
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

    // ================================================================
    // (참고) 기존 Result 버튼 제어 코드 - 현재 구조에서는 SubmitButton이 직접 제어하므로 기본적으로 사용 안 함
    // ================================================================
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
}

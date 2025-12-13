using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Stage Settings")]
    public int currentStage = 1;
    public int maxStage = 4;

    [Header("Camera Anchors")]
    public Transform camPosRadio; // 대화용 (라디오)
    public Transform camPosGame;  // 게임용 (플레이)

    // 🔥 [추가됨] 게임 플레이 중에만 보여야 할 UI들 (덱, 점수, 손패 UI 등)
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
    public GameObject continueButton;

    [Header("SFX")]
    public AudioClip gameStartSFX;
    public AudioClip stageStartSFX;
    public AudioClip stageClearSFX;
    public AudioClip postDialogueStartSFX;
    public AudioClip nextStageSFX;
    public AudioClip continueButtonClickSFX;

    [Header("Obstacle Audio Control")]
    public MovingTargetObstacle movingTarget;
    public ChainPendulum chainPendulum;

    private AudioSource audioSource;


    // 게임 상태 변수 (예시)
    private bool isStageEnded = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (continueButton != null)
            continueButton.SetActive(false);
    }

    private void Start()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayMainMenu();

        if (jokerStack == null) jokerStack = FindFirstObjectByType<JokerStack3D>();
        if (goalDeckManager == null) goalDeckManager = FindFirstObjectByType<GoalDeckManager>();
        if (startCardPicker == null) startCardPicker = FindFirstObjectByType<GameStartCardPicker>();
        if (handManager == null) handManager = FindFirstObjectByType<HandManager>();

        // 시작 시 카메라 텔레포트 및 UI 숨기기
        if (CameraDirector.Instance != null && camPosRadio != null)
        {
            CameraDirector.Instance.MoveToTarget(camPosRadio);
        }

        // 🔥 처음엔 대화부터 시작하니까 게임 UI는 꺼둡니다.
        SetGameUIActive(false);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true); // 메인 화면 켜기

            // (선택사항) 타이틀 화면에서는 커서가 보여야 함
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // 만약 메인 화면 캔버스를 연결 안 했으면 바로 시작 (테스트용)
            OnClickGameStart();
        }
    }


    public void OnClickGameStart()
    {
        Debug.Log("Game Start 버튼 눌림 -> 게임 시작!");

        // 🔊 게임 시작 버튼 효과음 (가장 먼저!)
        if (gameStartSFX != null)
            audioSource.PlayOneShot(gameStartSFX);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);

        // 🔊 메인 → 1스테이지 브금 전환 (여기가 핵심)
        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(1);

        if (CameraDirector.Instance != null && camPosRadio != null)
            CameraDirector.Instance.MoveToTarget(camPosRadio);

        EnterStageSequence(currentStage);
    }


    // -----------------------------------------------------
    // 스테이지 진입 (대화 시작)
    // -----------------------------------------------------
    public void EnterStageSequence(int stageIndex)
    {
        
        currentStage = Mathf.Clamp(stageIndex, 1, maxStage);

        if (BGMManager.Instance != null)
            BGMManager.Instance.PlayStage(currentStage);

        // 1. 카메라 이동 (Radio)
        if (CameraDirector.Instance != null && camPosRadio != null)
        {
            CameraDirector.Instance.MoveToTarget(camPosRadio);
        }

        // 🔥 2. 대화 중이므로 게임 UI 숨기기
        SetGameUIActive(false);

        StageDialogueProfile profile = GetDialogueProfile(currentStage);

        Debug.Log($"현재 스테이지 : {currentStage}");

        // 대화 시작
        if (DialogueManager.Instance != null && profile != null && profile.preStageDialogue.Count > 0)
        {
            Debug.Log($"💬 Stage {currentStage} 시작 전 대화");
            DialogueManager.Instance.StartDialogue(profile.preStageDialogue, () =>
            {
                SetupStageGameplay();
            });
        }
        else
        {
            SetupStageGameplay();
        }
    }

    // -----------------------------------------------------
    // 실제 게임 플레이 세팅
    // -----------------------------------------------------
    private void SetupStageGameplay()
    {
 


        // 🔊 스테이지 시작 사운드
        if (stageStartSFX != null)
            audioSource.PlayOneShot(stageStartSFX);

        // 1. 카메라 이동 (Game)
        if (CameraDirector.Instance != null && camPosGame != null)
        {
            CameraDirector.Instance.MoveToTarget(camPosGame);
        }
    


        // 🔥 2. 게임 시작! 게임 UI 표시
        SetGameUIActive(true);

        Debug.Log($"🎮 Stage {currentStage} 게임 플레이 시작");

        if (handManager != null) handManager.ClearSelectedCards3D();
        if (jokerStack != null) jokerStack.OnStageStart();
        if (goalDeckManager != null) goalDeckManager.SetupGoalForStage(currentStage);
        if (startCardPicker != null) startCardPicker.SetupForStage(currentStage);
        if (DeckManager.Instance != null) DeckManager.Instance.ResetDeckForNewStage();

        if (movingTarget != null)
            movingTarget.ResumeLoopSFX();

        if (chainPendulum != null)
            chainPendulum.ResumeLoopSFX();
    }

    // -----------------------------------------------------
    // 스테이지 클리어 처리
    // -----------------------------------------------------
    public void OnSubmitResult(bool isClear)
    {
        if (isClear)
        {
            if (movingTarget != null)
                movingTarget.StopLoopSFX();

            if (chainPendulum != null)
                chainPendulum.StopLoopSFX();

            // 🔊 스테이지 클리어 확정음
            if (stageClearSFX != null)
                audioSource.PlayOneShot(stageClearSFX);


            // 1. 카메라 이동 (Radio)
            if (CameraDirector.Instance != null && camPosRadio != null)
            {
                CameraDirector.Instance.MoveToTarget(camPosRadio);
            }

            // 🔥 2. 클리어 대화 시작하니까 게임 UI 다시 숨기기
            SetGameUIActive(false);

            StageDialogueProfile profile = GetDialogueProfile(currentStage);

            if (DialogueManager.Instance != null && profile != null && profile.postStageDialogue.Count > 0)
            {
                Debug.Log($"💬 Stage {currentStage} 클리어 후 대화");
                DialogueManager.Instance.StartDialogue(profile.postStageDialogue, () =>
                {
                    OnStageEnd();
                });
            }
            else
            {
                OnStageEnd();
            }
        }
    }

    public void GoToNextStage()
    {

        // 🔊 다음 스테이지 이동음
        if (nextStageSFX != null)
            audioSource.PlayOneShot(nextStageSFX);


        if (currentStage >= maxStage)
        {
            if (BGMManager.Instance != null)
                BGMManager.Instance.PlayEnding();

            Debug.Log($"🏆 모든 스테이지 클리어!");
            return;
        }

        EnterStageSequence(currentStage + 1);
    }

    // -----------------------------------------------------
    // 🔥 [헬퍼 함수] 게임 UI 껐다 켜기
    // -----------------------------------------------------
    private void SetGameUIActive(bool isActive)
    {
        if (gameUIGroup == null) return;

        foreach (var ui in gameUIGroup)
        {
            if (ui != null)
                ui.SetActive(isActive);
        }
    }

    private StageDialogueProfile GetDialogueProfile(int stage)
    {
        if (stageDialogues == null) return null;
        return stageDialogues.Find(x => x.stageIndex == stage);
    }

    public void OnStageEnd()
    {
        if (isStageEnded) return; // 중복 실행 방지
        isStageEnded = true;

        Debug.Log("스테이지 종료! Continue 버튼을 띄웁니다.");

        // 마우스 커서가 숨겨져 있었다면 다시 보이게 설정 (필요 시)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Continue 버튼 활성화
        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }
    }

    // ================================================================
    // UI 버튼에 연결할 함수 (Inspector의 OnClick에 연결하세요)
    // ================================================================
    public void OnClickContinue()
    {

        // 🔊 Continue 버튼 클릭음
        if (continueButtonClickSFX != null)
            audioSource.PlayOneShot(continueButtonClickSFX);


        Debug.Log("Continue 버튼 눌림 -> 스테이지 재시작");

        GoToNextStage();
    }
}
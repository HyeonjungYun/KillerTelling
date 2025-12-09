using System.Collections;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Stage Settings")]
    public int currentStage = 1;
    public int maxStage = 4;

    [Header("References")]
    public JokerStack3D jokerStack;
    public GoalDeckManager goalDeckManager;
    public GameStartCardPicker startCardPicker;
    public HandManager handManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (jokerStack == null) jokerStack = FindFirstObjectByType<JokerStack3D>();
        if (goalDeckManager == null) goalDeckManager = FindFirstObjectByType<GoalDeckManager>();
        if (startCardPicker == null) startCardPicker = FindFirstObjectByType<GameStartCardPicker>();
        if (handManager == null) handManager = FindFirstObjectByType<HandManager>();

        StartStage(currentStage);
    }

    // -----------------------------------------------------
    // 스테이지 시작
    // -----------------------------------------------------
    public void StartStage(int stageIndex)
    {
        currentStage = Mathf.Clamp(stageIndex, 1, maxStage);

        // 0) 플레이어 패 비우기 (이게 중요!)
        if (handManager != null)
            handManager.ClearSelectedCards3D();

        // 1) 조커 스택 재생성
        if (jokerStack != null)
            jokerStack.OnStageStart();

        // 2) 목표 덱/텍스트 세팅
        if (goalDeckManager != null)
            goalDeckManager.SetupGoalForStage(currentStage);

        // 3) 과녁에 걸릴 5장 세팅
        if (startCardPicker != null)
            startCardPicker.SetupForStage(currentStage);

        Debug.Log($"[StageManager] Stage {currentStage} 시작");
    }

    public void GoToNextStage()
    {
        if (currentStage >= maxStage)
        {
            Debug.Log("[StageManager] 마지막 스테이지 클리어! (엔딩 처리 TODO)");
            return;
        }

        StartStage(currentStage + 1);
    }

    // Submit에서 결과만 전달받아 로그용으로 사용
    public void OnSubmitResult(bool isClear)
    {
        Debug.Log($"[StageManager] Stage {currentStage} {(isClear ? "CLEAR" : "FAIL")}");
    }
}

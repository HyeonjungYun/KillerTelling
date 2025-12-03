using UnityEngine;

[System.Serializable] // 인스펙터에 노출시키기 위해 필수
public class StageScenario
{
    public string stageName;           // (옵션) 스테이지 이름 (구분용)
    public DialogueData introDialogue; // 게임 시작 전 대사
    public DialogueData clearDialogue; // 스테이지 클리어 후 대사
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    [Header("Story Data")]
    public StageScenario[] allStages; // 스테이지별 시나리오 배열 (Inspector에서 세팅)

    [Header("State")]
    public int currentStageIndex = 0; // 현재 진행 중인 스테이지

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        // 게임 시작하자마자 첫 스테이지 인트로 대화 시작
        PlayIntroDialogue();
    }

    // 1. 스테이지 시작 전 대화 틀기
    public void PlayIntroDialogue()
    {
        if (currentStageIndex < allStages.Length)
        {
            DialogueData data = allStages[currentStageIndex].introDialogue;
            if (data != null)
            {
                // "이 대화가 끝나면 StartGamePlay를 실행해줘"라고 예약
                DialogueManager.instance.OnDialogueEnded = StartGamePlay;
                DialogueManager.instance.StartDialogue(data);
            }
        }
    }

    // 2. 게임 플레이 시작 (대화가 끝난 후 호출)
    public void StartGamePlay()
    {
        Debug.Log($"스테이지 {currentStageIndex + 1} 게임 시작!");
        // 여기에 몬스터 스폰, 플레이어 조작 잠금 해제 등 게임 시작 로직 추가
    }

    // 3. 스테이지 클리어 시 호출 (외부에서 적을 다 잡으면 호출)
    public void OnStageClear()
    {
        if (currentStageIndex < allStages.Length)
        {
            DialogueData data = allStages[currentStageIndex].clearDialogue;
            if (data != null)
            {
                // "이 대화가 끝나면 NextStage를 실행해줘"라고 예약
                DialogueManager.instance.OnDialogueEnded = NextStage;
                DialogueManager.instance.StartDialogue(data);
            }
        }
    }

    // 4. 다음 스테이지로 넘어가기
    public void NextStage()
    {
        currentStageIndex++; // 인덱스 증가

        if (currentStageIndex < allStages.Length)
        {
            // 다음 스테이지 인트로 바로 시작 (또는 씬 로드)
            PlayIntroDialogue();
        }
        else
        {
            Debug.Log("모든 스테이지 클리어! 엔딩!");
            // 엔딩 씬 로드
        }
    }
}
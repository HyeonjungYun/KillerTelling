using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("대화 데이터")]
    public DialogueData dialogueData; // 실행할 대화 파일 연결

    [Header("설정")]
    public KeyCode triggerKey = KeyCode.Space; // 원하는 키 설정 (Space, F, Enter 등)

    void Update()
    {
        // 1. 키보드 입력 감지
        if (Input.GetKeyDown(triggerKey))
        {
            // 대화창이 꺼져 있을 때만 시작 (중복 실행 방지)
            if (!DialogueManager.instance.dialoguePanel.activeSelf)
            {
                StartMyDialogue();
            }
        }
    }

    // 2. 버튼과 키보드가 공통으로 호출할 함수
    public void StartMyDialogue()
    {
        DialogueManager.instance.StartDialogue(dialogueData);
    }
}
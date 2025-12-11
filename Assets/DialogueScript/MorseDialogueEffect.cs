using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class MorseDialogueEffect : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    public float decodeSpeed = 1.0f; // 한 글자가 해독되는 시간 간격
    public float startDelay = 1.0f;  // 처음에 전체 모스 부호를 보여주고 대기하는 시간
    [Tooltip("모스 부호 상태일 때의 색상 (HTML 컬러 코드)")]
    public string morseColorHex = "#00FF00"; // 매트릭스 같은 초록색

    // 대화 큐
    private Queue<string> sentences = new Queue<string>();
    private bool isAnimating = false;
    private string currentFullSentence = ""; // 현재 진행 중인 전체 문장 (스킵용)

    // 각 문자의 상태를 관리하기 위한 내부 클래스
    private class CharData
    {
        public char originalChar;
        public string morseCode;
        public bool isDecoded;
    }

    private List<CharData> currentCharDataList = new List<CharData>();
    private Coroutine currentRoutine;

    void Start()
    {
        dialoguePanel.SetActive(false);

        // --- 테스트용 (시작 시 자동 실행) ---
        // 실제 게임에서는 다른 스크립트에서 StartDialogue를 호출하세요.
        // StartDialogue(new string[] {
        //     "System Booting...",
        //     "Incoming transmission detected.",
        //     "Hello, Captain. Can you hear me?"
        // });
        // ------------------------------------
    }

    void Update()
    {
        // 클릭(또는 스페이스)으로 다음 대화 넘기기 / 스킵하기
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isAnimating)
            {
                // 연출 중 클릭하면 즉시 전체 해독 완료
                SkipAnimation();
            }
            else
            {
                // 연출이 끝났으면 다음 문장 표시
                DisplayNextSentence();
            }
        }
    }

    // 외부에서 이 함수를 호출해 대화 시작
    public void StartDialogue(string[] lines)
    {
        sentences.Clear();
        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentFullSentence = sentences.Dequeue();
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AnimateDecoding(currentFullSentence));
    }

    private IEnumerator AnimateDecoding(string sentence)
    {
        isAnimating = true;
        dialogueText.text = "";
        currentCharDataList.Clear();

        // 1. 문자열을 문자 단위 데이터로 변환
        foreach (char c in sentence)
        {
            currentCharDataList.Add(new CharData
            {
                originalChar = c,
                morseCode = MorseTranslator.GetMorse(c),
                isDecoded = false
            });
        }

        // 2. 최초 상태: 전체를 모스 부호로 표시
        UpdateDisplayText();

        // 처음에 모스 부호만 있는 상태로 잠시 대기 (긴장감 조성)
        yield return new WaitForSeconds(startDelay);

        // 3. 한 글자씩 해독 시작
        foreach (var charData in currentCharDataList)
        {
            // 공백이나 특수문자는 해독 과정 없이 바로 보여줄 수도 있음 (선택 사항)
            // 여기서는 모든 문자를 순차적으로 해독

            charData.isDecoded = true; // 상태 변경 (모스 -> 원래 문자)
            UpdateDisplayText();       // 화면 갱신

            // 효과음 재생 위치 (필요시)
            // AudioManager.Play("DecodeBeep"); 

            yield return new WaitForSeconds(decodeSpeed);
        }

        isAnimating = false;
    }

    // 현재 CharDataList 상태를 기반으로 텍스트를 조합해서 화면에 뿌림
    private void UpdateDisplayText()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var data in currentCharDataList)
        {
            if (data.isDecoded)
            {
                // 해독된 문자 (흰색)
                sb.Append(data.originalChar);
            }
            else
            {
                // 아직 해독 안 된 모스 부호 (설정된 색상)
                // 모스 부호끼리 구분을 위해 뒤에 공백 추가
                sb.Append($"<color={morseColorHex}>{data.morseCode} </color>");
            }
        }

        dialogueText.text = sb.ToString();
    }

    // 연출 스킵 기능
    private void SkipAnimation()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        dialogueText.text = currentFullSentence; // 즉시 완성된 문장 보여줌
        isAnimating = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        Debug.Log("대화 종료");
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro 사용 필수
using UnityEditor.Rendering.PostProcessing;
using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI Components")]
    public GameObject dialoguePanel; // 대화창 전체 부모
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDialogue;

    [Header("Settings")]
    public float typingSpeed = 0.05f; // 모스부호 나오는 속도
    public float decodeDelay = 1.0f;  // 해석 대기 시간

    [Tooltip("한 글자씩 해석될 때 걸리는 시간")]
    public float decodingSpeed = 10.0f;

    private Queue<string> sentences = new Queue<string>();
    private bool isTyping = false;

    public Action OnDialogueEnded;

    void Awake()
    {
        if (instance == null) instance = this;
        dialoguePanel.SetActive(false); // 시작할 땐 꺼둠
    }

    public void StartDialogue(DialogueData dialogue)
    {
        // 1. 카메라를 뒤쪽(대화 뷰)으로 회전!
        ViewManager.instance.SwitchToDialogueMode();

        // 2. 대화 패널 켜기
        dialoguePanel.SetActive(true);
        textName.text = dialogue.speakerName;

        // (데이터 큐 채우기 로직은 그대로 유지...)
        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        // 회전하는 동안 바로 글자가 나오면 어색하니까, 
        // 카메라가 다 돌아간 뒤(1초 뒤)에 첫 대사가 나오게 딜레이를 줍니다.
        Invoke("DisplayNextSentence", 1.0f);
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        ViewManager.instance.SwitchToGameMode(); // 카메라 복귀

        // ★★★ 구독자들에게 "대화 끝났어!"라고 알림 ★★★
        if (OnDialogueEnded != null)
        {
            OnDialogueEnded.Invoke();
            OnDialogueEnded = null; // 한번 쓰고 초기화 (중요: 이전 이벤트가 남지 않게)
        }
    }

    public void DisplayNextSentence()
    {
        if (isTyping) return;

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string originalText = sentences.Dequeue();

        // 이제 MorseTranslator를 여기서 호출할 필요 없이 원본만 넘기면 됩니다.
        StartCoroutine(TypeMorseAndDecode(originalText));
    }

    IEnumerator TypeMorseAndDecode(string originalText)
    {
        isTyping = true;
        textDialogue.text = "";

        // 1. 원본 텍스트를 모스 부호 배열로 변환
        // 예: "가나" -> ["(가 모스) ", "(나 모스) "]
        string[] morseArray = MorseTranslator.TextToMorseArray(originalText);
    
        // 전체 모스 부호 문자열 만들기 (Typing용)
        string fullMorse = string.Join("", morseArray);

        // --- Phase 1: 모스 부호 타이핑 ---
        foreach (char letter in fullMorse.ToCharArray())
        {
            textDialogue.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // --- Phase 2: 대기 ---
        yield return new WaitForSeconds(decodeDelay);

        // --- Phase 3: 한 글자씩 해석 (Decoding) ---

        string decodedPart = "";
        string remainingMorse = fullMorse;

        for (int i = 0; i < originalText.Length; i++)
        {
            string targetMorse = morseArray[i];

            if (remainingMorse.Length >= targetMorse.Length)
            {
                remainingMorse = remainingMorse.Substring(targetMorse.Length);
            }

            decodedPart += originalText[i];
            textDialogue.text = decodedPart + remainingMorse;

            // [변경] 기존 typingSpeed * 2f 대신 decodingSpeed 변수 사용
            yield return new WaitForSeconds(decodingSpeed);
        }

        textDialogue.text = originalText;
        isTyping = false;
}

    void Update()
    {
        // 대화창이 켜져 있을 때만 작동
        if (dialoguePanel.activeSelf)
        {
            // 마우스 클릭 OR 스페이스바(또는 원하는 키) 입력 시
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (!isTyping)
                {
                    DisplayNextSentence();
                }
            }
        }
    }
}
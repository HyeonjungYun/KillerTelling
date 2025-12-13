using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI contentText;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    [Tooltip("소리의 높낮이 (Hz). 기본 450Hz")]
    [Range(200, 1000)] public int toneFrequency = 450;

    private AudioClip dotSound;
    private AudioClip dashSound;

    [Header("Morse Effect Settings")]
    public float dotSpeed = 0.08f;
    public float dashSpeed = 0.2f;
    public float startDelay = 0.5f;
    public float decodeSpeed = 0.05f;
    public string morseColorHex = "#00FF00";

    // 내부 데이터 구조
    private class CharData
    {
        public char originalChar;   // 화면에 나올 글자 ('안')
        public string targetMorse;  // 소리/모양 낼 모스 부호 ('...---...')
        public string currentMorse;
        public bool isDecoded;
    }

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private Action onDialogueComplete;
    private bool isDialogueActive = false;
    private bool isAnimating = false;
    private List<CharData> currentCharDataList = new List<CharData>();
    private string currentFullText = "";
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        GenerateBeepSounds();
    }

    private void GenerateBeepSounds()
    {
        dotSound = AudioClipFactory.CreateBeep("Dot", toneFrequency, 0.08f);
        dashSound = AudioClipFactory.CreateBeep("Dash", toneFrequency, 0.24f);
    }

    private void OnValidate()
    {
        if (Application.isPlaying) GenerateBeepSounds();
    }

    private void Update()
    {
        if (isDialogueActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isAnimating) SkipAnimation();
            else DisplayNextLine();
        }
    }

    public void StartDialogue(List<DialogueLine> lines, Action onComplete)
    {
        Debug.Log($"[StartDialogue] lines={lines?.Count}, isDialogueActive={isDialogueActive}");
        Debug.Log("A"); // 조건 진입
        Debug.Log("B"); // 패널 활성화 직전
        Debug.Log("C"); // 코루틴 시작 직전
        if (lines == null || lines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        isDialogueActive = true;
        onDialogueComplete = onComplete;
        dialogueQueue.Clear();

        foreach (var line in lines) dialogueQueue.Enqueue(line);

        if (CardGraveyardManager.Instance != null)
            CardGraveyardManager.IsInputBlocked = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        // 🔥 DialogueLine 객체 통째로 가져오기 (Override 확인용)
        DialogueLine line = dialogueQueue.Dequeue();
        currentFullText = line.content;

        if (currentRoutine != null) StopCoroutine(currentRoutine);

        // 🔥 line 객체를 넘겨줌
        currentRoutine = StartCoroutine(AnimateDecoding(line));
    }

    // =========================================================
    // 🔥 핵심: 보여줄 글자와 모스 부호용 글자를 분리
    // =========================================================
    private IEnumerator AnimateDecoding(DialogueLine line)
    {
        isAnimating = true;
        if (contentText != null) contentText.text = "";
        currentCharDataList.Clear();

        string visibleText = line.content;
        string morseSourceText = line.morseOverride;

        // Morse Override가 비어있으면 원문 그대로 사용
        bool useOverride = !string.IsNullOrEmpty(morseSourceText);

        for (int i = 0; i < visibleText.Length; i++)
        {
            char visibleChar = visibleText[i];
            char morseChar = visibleChar; // 기본값은 원문

            // 🔥 Override가 있다면 해당 글자를 사용
            if (useOverride)
            {
                // 글자 수가 안 맞을 경우를 대비해 반복(Loop) 시킴
                // 예: 원문 "안녕하세요" (5글자), 오버라이드 "SOS" (3글자)
                // 안(S), 녕(O), 하(S), 세(S), 요(O) ... 
                morseChar = morseSourceText[i % morseSourceText.Length];
            }

            currentCharDataList.Add(new CharData
            {
                originalChar = visibleChar,
                // 🔥 보여줄 글자(visibleChar)가 아니라 모스용 글자(morseChar)로 변환
                targetMorse = MorseTranslator.GetMorse(morseChar),
                currentMorse = "",
                isDecoded = false
            });
        }

        // [Typing Phase]
        foreach (var data in currentCharDataList)
        {
            foreach (char symbol in data.targetMorse)
            {
                data.currentMorse += symbol;

                if (symbol == '.')
                {
                    PlaySound(dotSound);
                    UpdateDisplayText();
                    yield return new WaitForSeconds(dotSpeed);
                }
                else if (symbol == '-')
                {
                    PlaySound(dashSound);
                    UpdateDisplayText();
                    yield return new WaitForSeconds(dashSpeed);
                }
                else
                {
                    UpdateDisplayText();
                    yield return new WaitForSeconds(dotSpeed);
                }
            }
            data.currentMorse += " ";
            UpdateDisplayText();
        }

        yield return new WaitForSeconds(startDelay);

        // [Decoding Phase]
        foreach (var charData in currentCharDataList)
        {
            charData.isDecoded = true;
            UpdateDisplayText();
            yield return new WaitForSeconds(decodeSpeed);
        }

        isAnimating = false;
    }

    private void UpdateDisplayText()
    {
        if (contentText == null) return;
        StringBuilder sb = new StringBuilder();
        foreach (var data in currentCharDataList)
        {
            if (data.isDecoded) sb.Append(data.originalChar);
            else sb.Append($"<color={morseColorHex}>{data.currentMorse}</color>");
        }
        contentText.text = sb.ToString();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.98f, 1.02f);
            audioSource.PlayOneShot(clip, soundVolume);
        }
    }

    private void SkipAnimation()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        if (contentText != null) contentText.text = currentFullText;
        isAnimating = false;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isAnimating = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (CardGraveyardManager.Instance != null)
            CardGraveyardManager.IsInputBlocked = false;
        onDialogueComplete?.Invoke();
    }
}
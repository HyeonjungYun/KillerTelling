using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SubmitButton : MonoBehaviour
{
    public Button submitButton;
    public Transform selectedCard3DSpawnPoint;
    public GoalDeckManager goalDeckManager;
    public GameObject resultSlotPrefab;

    [Header("Result UI")]
    public GameObject resultCanvas;
    public TextMeshProUGUI resultCanvasText;

    [Header("Result Buttons")]
    public Button mainButton;   // GoHomeButton (항상 표시)
    public Button checkButton;  // CheckButton (클리어 시에만 표시)

    [Header("Result Cards")]
    public Transform playerCardArea;
    public Transform goalCardArea;
    public GameObject resultCardPrefab;

    [Header("Result Background")]
    public Image resultBackgroundImage;
    public Sprite clearBackgroundSprite;
    public Sprite failBackgroundSprite;


    // =========================================================
    // 🔊 (추가) SFX / BGM
    // =========================================================
    [Header("SFX")]
    public AudioClip submitClickSFX;
    public AudioClip resultClearSFX;
    public AudioClip resultFailSFX;
    public AudioClip cardAppearSFX;
    public AudioClip checkButtonClickSFX;
    public AudioClip mainButtonClickSFX;

    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("BGM Control (Optional)")]
    [Tooltip("제출 시 기존 BGM을 잠시 멈출지")]
    public bool pauseBgmOnSubmit = true;

    [Tooltip("결과창이 닫힐 때(클리어 시) BGM을 다시 재생할지")]
    public bool resumeBgmOnCloseIfClear = true;

    private AudioSource audioSource;

    private void EnsureAudio()
    {
        if (audioSource != null) return;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = sfxVolume;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        EnsureAudio();
        audioSource.volume = sfxVolume;
        audioSource.PlayOneShot(clip);
    }

    // =========================================================

    private bool hasSubmittedThisStage = false;
    private bool lastIsClear = false;

    private int cachedStageIndex = -1;
    private bool cachedTutorialPhase = false; // ✅ 튜토/실전 전환 감지

    private CanvasGroup resultCanvasGroup;
    private bool resultUIReadyForClick = false;

    // ✅ 조커 0 자동제출 중복 방지
    private bool autoSubmittedByJokerDepletedThisStage = false;

    private void Start()
    {
        EnsureAudio();

        // ✅ 안전장치: submitButton 자동 연결
        if (submitButton == null)
            submitButton = GetComponent<Button>();

        // ✅ 안전장치: selectedCard3DSpawnPoint 자동 연결
        EnsureSelectedSpawnPointRef();

        UpdateButtonState();

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmit);

        if (goalDeckManager == null)
            goalDeckManager = FindFirstObjectByType<GoalDeckManager>();

        if (resultCanvas != null)
        {
            resultCanvas.SetActive(false);

            resultCanvasGroup = resultCanvas.GetComponent<CanvasGroup>();
            if (resultCanvasGroup == null)
                resultCanvasGroup = resultCanvas.AddComponent<CanvasGroup>();
        }

        if (checkButton != null)
            checkButton.onClick.AddListener(OnResultCheckButtonPressed);

        // (추가) mainButton도 클릭음 필요하면 연결
        if (mainButton != null)
            mainButton.onClick.AddListener(OnMainButtonPressed);

        HideResultButtonsImmediate();
    }

    private void Update()
    {
        // ✅ 튜토/실전에서 spawnPoint가 씬 리셋으로 끊길 수 있으니 매 프레임 보정
        EnsureSelectedSpawnPointRef();

        UpdateButtonState();

        int stage = StageManager.Instance != null ? StageManager.Instance.currentStage : -1;
        bool tutorialPhase = StageManager.Instance != null && StageManager.Instance.IsStage1TutorialPhase;

        // ✅ (핵심) stage가 같아도 튜토↔실전이 바뀌면 제출 플래그 리셋
        if (stage != cachedStageIndex || tutorialPhase != cachedTutorialPhase)
        {
            cachedStageIndex = stage;
            cachedTutorialPhase = tutorialPhase;

            hasSubmittedThisStage = false;
            autoSubmittedByJokerDepletedThisStage = false;
        }

        // 1) 5장이 되면 자동 제출
        if (!hasSubmittedThisStage &&
            selectedCard3DSpawnPoint != null &&
            selectedCard3DSpawnPoint.childCount == 5)
        {
            OnSubmit();
            return;
        }

        // 2) 조커 0이면 자동 제출(패가 부족해도 FAIL로 결과 표시)
        if (!hasSubmittedThisStage &&
            !autoSubmittedByJokerDepletedThisStage &&
            IsJokerDepleted())
        {
            autoSubmittedByJokerDepletedThisStage = true;
            OnSubmit(forceSubmitEvenIfNotEnoughCards: true);
            return;
        }
    }

    private void EnsureSelectedSpawnPointRef()
    {
        if (selectedCard3DSpawnPoint == null && HandManager.Instance != null)
            selectedCard3DSpawnPoint = HandManager.Instance.selectedCard3DSpawnPoint;
    }

    private bool IsJokerDepleted()
    {
        if (JokerStack3D.Instance == null) return false;
        return JokerStack3D.Instance.CurrentJoker <= 0;
    }

    private void UpdateButtonState()
    {
        if (submitButton == null || selectedCard3DSpawnPoint == null)
            return;

        // ✅ 기존 규칙 유지: 최소 2장일 때만 제출 버튼 활성
        submitButton.interactable = selectedCard3DSpawnPoint.childCount >= 2;
    }

    // -----------------------------------------------------
    // 제출 처리
    // -----------------------------------------------------
    private void OnSubmit()
    {
        OnSubmit(forceSubmitEvenIfNotEnoughCards: false);
    }

    private void OnSubmit(bool forceSubmitEvenIfNotEnoughCards)
    {
        // ✅ 결과 UI 떠있는 동안에는 추가 제출 방지(클릭 오작동 방지)
        if (resultCanvas != null && resultCanvas.activeSelf)
            return;

        if (hasSubmittedThisStage) return;
        hasSubmittedThisStage = true;

        // 🔊 제출 클릭음
        PlaySfx(submitClickSFX);

        // 🎵 (선택) 제출 시 BGM 멈춤
        if (pauseBgmOnSubmit && BGMManager.Instance != null)
            BGMManager.Instance.Pause();

        EnsureSelectedSpawnPointRef();

        int count = selectedCard3DSpawnPoint != null ? selectedCard3DSpawnPoint.childCount : 0;

        // 기본은 2장 미만 제출 불가, 단 조커0 자동제출(force) 상황이면 FAIL 결과창 띄움
        if (count < 2 && !forceSubmitEvenIfNotEnoughCards)
        {
            Debug.Log("❌ 최소 2장 필요!");
            hasSubmittedThisStage = false;
            return;
        }

        if (goalDeckManager == null)
        {
            Debug.LogError("GoalDeckManager 연결 안됨");
            hasSubmittedThisStage = false;
            return;
        }

        List<CardData> goalDeck = goalDeckManager.GetGoalDeckAsCardData();

        // 플레이어 덱 수집
        List<CardData> playerDeck = new List<CardData>();
        if (selectedCard3DSpawnPoint != null)
        {
            foreach (Transform t in selectedCard3DSpawnPoint)
            {
                Card3D c = t.GetComponent<Card3D>();
                if (c != null && c.cardData != null)
                    playerDeck.Add(c.cardData);
            }
        }

        // 2장 미만 + force 제출이면 NO HAND 처리(FAIL 보장)
        string playerRank;
        int playerValue;

        if (playerDeck.Count < 2)
        {
            playerRank = "NO HAND";
            playerValue = -999999;
        }
        else
        {
            playerRank = DeckEvaluator.EvaluateDeck(playerDeck);
            playerValue = DeckEvaluator.GetRankValue(playerRank);
        }

        string goalRank = DeckEvaluator.EvaluateDeck(goalDeck);
        int goalValue = DeckEvaluator.GetRankValue(goalRank);

        bool isClear = playerValue >= goalValue;
        lastIsClear = isClear;

        // 🔊 결과 SFX
        PlaySfx(isClear ? resultClearSFX : resultFailSFX);

        // ✅ StageManager에 결과 통보
        if (StageManager.Instance != null)
            StageManager.Instance.OnSubmitResult(isClear);

        StartCoroutine(ShowResultUIAfterOneFrame(isClear, playerRank, goalRank, playerDeck, goalDeck));
    }

    private IEnumerator ShowResultUIAfterOneFrame(
        bool isClear,
        string playerRank,
        string goalRank,
        List<CardData> playerDeck,
        List<CardData> goalDeck
    )
    {
        if (resultCanvas != null)
            resultCanvas.SetActive(true);

        if (resultBackgroundImage != null)
        {
            resultBackgroundImage.sprite =
                isClear ? clearBackgroundSprite : failBackgroundSprite;
        }

        if (resultCanvasText != null)
        {
            bool jokerDepleted = IsJokerDepleted();
            bool notEnough = (playerDeck == null || playerDeck.Count < 2);

            string extra =
                (jokerDepleted && notEnough)
                ? "\n\n<color=#FF6666>Jokers depleted. Not enough cards to submit.</color>"
                : "";

            resultCanvasText.text =
                $"<size=50><b>RESULT</b></size>\n" +
                $"Player : {playerRank}                   Goal : {goalRank}\n\n\n\n\n\n" +
                (isClear ? "<color=#FFD700><size=55><b>CLEAR!</b></size></color>"
                         : "<color=red><size=55><b>FAIL</b></size></color>") +
                extra;
        }

        // 메인 버튼은 항상 표시
        if (mainButton != null) mainButton.gameObject.SetActive(true);

        // ✅ Check 버튼은 "클리어"면 표시
        if (checkButton != null) checkButton.gameObject.SetActive(isClear);

        resultUIReadyForClick = false;
        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.blocksRaycasts = false;
            resultCanvasGroup.interactable = false;
        }

        StartCoroutine(ShowPlayerCards(playerDeck));
        StartCoroutine(ShowGoalCards(goalDeck));

        // ✅ 실제 3D 패는 여기서 비우기
        if (HandManager.Instance != null)
            HandManager.Instance.ClearSelectedCards3D();

        yield return null;
        yield return new WaitForEndOfFrame();

        if (resultCanvasGroup != null)
        {
            resultCanvasGroup.blocksRaycasts = true;
            resultCanvasGroup.interactable = true;
        }
        resultUIReadyForClick = true;
    }

    private void HideResultButtonsImmediate()
    {
        if (mainButton != null) mainButton.gameObject.SetActive(false);
        if (checkButton != null) checkButton.gameObject.SetActive(false);
    }

    // -----------------------------------------------------
    // 결과창 Main 버튼 (항상 표시) - 클릭음만 추가
    // -----------------------------------------------------
    private void OnMainButtonPressed()
    {
        // 🔊 메인 버튼 클릭음
        PlaySfx(mainButtonClickSFX);

        // 실제 메인 이동 로직은 기존 GoHomeButton 쪽에서 처리될 가능성이 높아서
        // 여기서는 "사운드만" 담당하도록 둠.
        // (원하면 여기서도 resultCanvas 닫기 같은 처리 추가 가능)
    }

    private void OnResultCheckButtonPressed()
    {
        if (!resultUIReadyForClick) return;

        // 🔊 체크 버튼 클릭음
        PlaySfx(checkButtonClickSFX);

        if (StageManager.Instance == null)
        {
            Debug.LogWarning("StageManager.Instance 없음");
            return;
        }

        // ✅ 튜토리얼 클리어 pending 처리
        if (StageManager.Instance.HasPendingTutorialClear)
        {
            StageManager.Instance.ConfirmTutorialClearAndStartStage1();

            if (resultCanvas != null)
                resultCanvas.SetActive(false);

            // ❌ 튜토리얼 → Stage1 전환에서는 Resume 하면 안 됨
            // StageManager 쪽에서 PlayStage(1)을 책임지게 둔다

            hasSubmittedThisStage = false;
            autoSubmittedByJokerDepletedThisStage = false;

            return;
        }



        // ✅ 일반 스테이지: 클리어면 다음 스테이지, 실패면 종료
        if (lastIsClear)
        {
            StageManager.Instance.GoToNextStage();

            // 🎵 (선택) 클리어 후 결과창 닫힐 때 BGM 재개
            if (resumeBgmOnCloseIfClear && BGMManager.Instance != null)
                BGMManager.Instance.Resume();
        }
        else
        {
            // 실패면 보통 BGM 끄는 게 자연스러워서 Stop
            if (BGMManager.Instance != null)
                BGMManager.Instance.Stop();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (resultCanvas != null)
            resultCanvas.SetActive(false);
    }

    // -----------------------------------------------------
    // 결과창: 플레이어 패 연출 (+ 카드 등장 SFX)
    // -----------------------------------------------------
    private IEnumerator ShowPlayerCards(List<CardData> deck)
    {
        if (playerCardArea == null) yield break;

        foreach (Transform c in playerCardArea) Destroy(c.gameObject);

        float startX = -300f;
        float gapX = 60f;
        float y = 0f;

        List<RectTransform> slotPositions = new List<RectTransform>();

        for (int i = 0; i < 5; i++)
        {
            if (resultSlotPrefab == null) break;

            GameObject slot = Instantiate(resultSlotPrefab, playerCardArea);
            RectTransform rt = slot.GetComponent<RectTransform>();
      
            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = new Vector2(startX + i * gapX, y);

            slotPositions.Add(rt);
        }

        if (deck == null) yield break;

        for (int i = 0; i < deck.Count && i < slotPositions.Count; i++)
        {
            if (resultCardPrefab == null) yield break;

            // 🔊 카드 등장음
            PlaySfx(cardAppearSFX);

            GameObject card = Instantiate(resultCardPrefab, playerCardArea);
            RectTransform rt = card.GetComponent<RectTransform>();
            DisableHover(card);

            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = slotPositions[i].anchoredPosition;

            Image img = card.GetComponent<Image>();
            if (img != null) img.sprite = deck[i].sprite;

            card.transform.localScale = Vector3.zero;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                card.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    // -----------------------------------------------------
    // 결과창: 목표 덱 연출 (+ 카드 등장 SFX)
    // -----------------------------------------------------
    private IEnumerator ShowGoalCards(List<CardData> deck)
    {
        if (goalCardArea == null) yield break;

        foreach (Transform c in goalCardArea) Destroy(c.gameObject);

        float startX = 100f;
        float gapX = 60f;
        float y = 0f;

        List<RectTransform> slotPositions = new List<RectTransform>();

        for (int i = 0; i < 5; i++)
        {
            if (resultSlotPrefab == null) break;

            GameObject slot = Instantiate(resultSlotPrefab, goalCardArea);
            RectTransform rt = slot.GetComponent<RectTransform>();

            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = new Vector2(startX + i * gapX, y);

            slotPositions.Add(rt);
        }

        if (deck == null) yield break;

        for (int i = 0; i < deck.Count && i < slotPositions.Count; i++)
        {
            if (resultCardPrefab == null) yield break;

            // 🔊 카드 등장음
            PlaySfx(cardAppearSFX);

            GameObject card = Instantiate(resultCardPrefab, goalCardArea);
            RectTransform rt = card.GetComponent<RectTransform>();
            DisableHover(card);

            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = slotPositions[i].anchoredPosition;

            Image img = card.GetComponent<Image>();
            if (img != null) img.sprite = deck[i].sprite;

            card.transform.localScale = Vector3.zero;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                card.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void DisableHover(GameObject card)
    {
        // Hover 관련 MonoBehaviour만 골라서 비활성화
        foreach (var comp in card.GetComponents<MonoBehaviour>())
        {
            if (comp.GetType().Name.Contains("Hover"))
            {
                comp.enabled = false;
            }
        }
    }

}

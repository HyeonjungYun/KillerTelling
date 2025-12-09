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
    public Button checkButton;

    [Header("Result Cards")]
    public Transform playerCardArea;
    public Transform goalCardArea;
    public GameObject resultCardPrefab;

    // 상태 플래그
    private bool hasSubmittedThisStage = false;
    private bool lastIsClear = false;
    private int cachedStageIndex = -1;

    private void Start()
    {
        UpdateButtonState();

        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmit);

        if (goalDeckManager == null)
            goalDeckManager = FindFirstObjectByType<GoalDeckManager>();

        if (resultCanvas != null)
            resultCanvas.SetActive(false);

        if (checkButton != null)
            checkButton.onClick.AddListener(OnResultCheckButtonPressed);
    }

    private void Update()
    {
        UpdateButtonState();

        // 스테이지가 바뀌면 자동 제출 플래그 리셋
        int stage = StageManager.Instance != null ? StageManager.Instance.currentStage : -1;
        if (stage != cachedStageIndex)
        {
            cachedStageIndex = stage;
            hasSubmittedThisStage = false;
        }

        // 5장이 되면 "해당 스테이지에서 한 번만" 자동 제출
        if (!hasSubmittedThisStage &&
            selectedCard3DSpawnPoint != null &&
            selectedCard3DSpawnPoint.childCount == 5)
        {
            OnSubmit();
        }
    }

    private void UpdateButtonState()
    {
        if (selectedCard3DSpawnPoint == null || submitButton == null)
            return;

        submitButton.interactable = selectedCard3DSpawnPoint.childCount >= 2;
    }

    // -----------------------------------------------------
    // 제출 처리
    // -----------------------------------------------------
    private void OnSubmit()
    {
        if (hasSubmittedThisStage) return;     // 두 번 제출 방지
        hasSubmittedThisStage = true;

        int count = selectedCard3DSpawnPoint.childCount;

        if (count < 2)
        {
            Debug.Log("❌ 최소 2장 필요!");
            hasSubmittedThisStage = false;    // 다시 제출할 수 있게
            return;
        }

        List<CardData> playerDeck = new List<CardData>();
        foreach (Transform t in selectedCard3DSpawnPoint)
        {
            Card3D c = t.GetComponent<Card3D>();
            if (c != null && c.cardData != null)
                playerDeck.Add(c.cardData);
        }

        if (goalDeckManager == null)
        {
            Debug.LogError("GoalDeckManager 연결 안됨");
            return;
        }

        List<CardData> goalDeck = goalDeckManager.GetGoalDeckAsCardData();

        string playerRank = DeckEvaluator.EvaluateDeck(playerDeck);
        string goalRank = DeckEvaluator.EvaluateDeck(goalDeck);

        int playerValue = DeckEvaluator.GetRankValue(playerRank);
        int goalValue = DeckEvaluator.GetRankValue(goalRank);

        bool isClear = playerValue >= goalValue;
        lastIsClear = isClear;

        // StageManager에 결과 통보 (로그용)
        if (StageManager.Instance != null)
            StageManager.Instance.OnSubmitResult(isClear);

        // 결과 UI
        if (resultCanvas != null)
        {
            resultCanvas.SetActive(true);
            resultCanvasText.text =
                $"<size=50><b>RESULT</b></size>\n" +
                $"Player : {playerRank}                   Goal : {goalRank}\n\n\n\n\n\n" +
                (isClear ? "<color=#FFD700><size=55><b>CLEAR!</b></size></color>"
                         : "<color=red><size=55><b>FAIL</b></size></color>");
        }

        StartCoroutine(ShowPlayerCards(playerDeck));
        StartCoroutine(ShowGoalCards(goalDeck));

        // ⚠ 핵심: 실제 3D 패는 여기서 바로 비워 둔다
        if (HandManager.Instance != null)
            HandManager.Instance.ClearSelectedCards3D();
    }

    // -----------------------------------------------------
    // 결과창 Check 버튼
    // -----------------------------------------------------
    private void OnResultCheckButtonPressed()
    {
        if (StageManager.Instance == null)
        {
            Debug.LogWarning("StageManager.Instance 없음");
            return;
        }

        if (lastIsClear)
        {
            // 클리어 → 다음 스테이지
            StageManager.Instance.GoToNextStage();
        }
        else
        {
            // 실패 → 임시로 게임 종료
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
    // 결과창: 플레이어 패 연출
    // -----------------------------------------------------
    private IEnumerator ShowPlayerCards(List<CardData> deck)
    {
        foreach (Transform c in playerCardArea) Destroy(c.gameObject);

        float startX = -300f;
        float gapX = 60f;
        float y = 0f;

        List<RectTransform> slotPositions = new List<RectTransform>();

        for (int i = 0; i < 5; i++)
        {
            GameObject slot = Instantiate(resultSlotPrefab, playerCardArea);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = new Vector2(startX + i * gapX, y);

            slotPositions.Add(rt);
        }

        for (int i = 0; i < deck.Count; i++)
        {
            GameObject card = Instantiate(resultCardPrefab, playerCardArea);
            RectTransform rt = card.GetComponent<RectTransform>();

            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = slotPositions[i].anchoredPosition;

            card.GetComponent<Image>().sprite = deck[i].sprite;

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
    // 결과창: 목표 덱 연출
    // -----------------------------------------------------
    private IEnumerator ShowGoalCards(List<CardData> deck)
    {
        foreach (Transform c in goalCardArea) Destroy(c.gameObject);

        float startX = 100f;
        float gapX = 60f;
        float y = 0f;

        List<RectTransform> slotPositions = new List<RectTransform>();

        for (int i = 0; i < 5; i++)
        {
            GameObject slot = Instantiate(resultSlotPrefab, goalCardArea);
            RectTransform rt = slot.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = new Vector2(startX + i * gapX, y);

            slotPositions.Add(rt);
        }

        for (int i = 0; i < deck.Count; i++)
        {
            GameObject card = Instantiate(resultCardPrefab, goalCardArea);
            RectTransform rt = card.GetComponent<RectTransform>();

            rt.sizeDelta = new Vector2(55f, 75f);
            rt.anchoredPosition = slotPositions[i].anchoredPosition;

            card.GetComponent<Image>().sprite = deck[i].sprite;

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
}

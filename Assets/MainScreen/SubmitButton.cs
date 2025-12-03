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

    [Header("Result Cards")]
    public Transform playerCardArea;
    public Transform goalCardArea;
    public GameObject resultCardPrefab;

    private void Start()
    {
        UpdateButtonState();
        submitButton.onClick.AddListener(OnSubmit);

        if (goalDeckManager == null)
            goalDeckManager = FindObjectOfType<GoalDeckManager>();

        if (resultCanvas != null)
            resultCanvas.SetActive(false);
    }

    private void Update()
    {
        UpdateButtonState();

        if (selectedCard3DSpawnPoint.childCount == 5)
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

    private void OnSubmit()
    {
        int count = selectedCard3DSpawnPoint.childCount;

        if (count < 2)
        {
            Debug.Log("❌ 최소 2장 필요!");
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
    }

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



/*using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SubmitButton : MonoBehaviour
{
    public Button submitButton;
    public Transform selectedCard3DSpawnPoint;
    public TextMeshProUGUI resultText;

    public GoalDeckManager goalDeckManager;  // ⭐ 목표덱 참조 추가
 

    private void Start()
    {

        UpdateButtonState();
        submitButton.onClick.AddListener(OnSubmit);
        if (goalDeckManager == null)
            goalDeckManager = FindObjectOfType<GoalDeckManager>();

    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (selectedCard3DSpawnPoint == null || submitButton == null)
            return;

        submitButton.interactable = selectedCard3DSpawnPoint.childCount >= 2;
    }

    private void OnSubmit()
    {
        int count = selectedCard3DSpawnPoint.childCount;

        if (count < 2)
        {
            Debug.Log("❌ 제출 불가: 최소 2장 필요!");
            return;
        }

        Debug.Log("📤 제출 버튼 클릭");

        // --------------------------------
        // 1) 플레이어 손패 CardData 수집
        // --------------------------------
        List<CardData> playerDeck = new List<CardData>();
        foreach (Transform t in selectedCard3DSpawnPoint)
        {
            Card3D card3D = t.GetComponent<Card3D>();
            if (card3D != null && card3D.cardData != null)
                playerDeck.Add(card3D.cardData);
        }

        // --------------------------------
        // 2) 목표 덱 가져오기
        // --------------------------------
        if (goalDeckManager == null)
        {
            Debug.LogError("❌ GoalDeckManager가 SubmitButton에 연결되지 않음!");
            return;
        }

        List<CardData> goalDeck = goalDeckManager.GetGoalDeckAsCardData();

        // --------------------------------
        // 3) 각각 평가
        // --------------------------------
        string playerRank = DeckEvaluator.EvaluateDeck(playerDeck);
        string goalRank = DeckEvaluator.EvaluateDeck(goalDeck);

        int playerValue = DeckEvaluator.GetRankValue(playerRank);
        int goalValue = DeckEvaluator.GetRankValue(goalRank);

        bool isClear = playerValue >= goalValue;

        // --------------------------------
        // 4) UI 출력
        // --------------------------------
        if (resultText != null)
        {
            resultText.text =
                $"Player: {playerRank}\n" +
                $"Goal: {goalRank}\n" +
                $"Result: {(isClear ? "<color=yellow>CLEAR!</color>" : "<color=red>FAIL</color>")}";
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}*/

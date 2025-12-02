using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SubmitButton : MonoBehaviour
{
    public Button submitButton;
    public Transform selectedCard3DSpawnPoint;

    public GoalDeckManager goalDeckManager;

    [Header("Result UI")]
    public GameObject resultCanvas;               // 🔥 새 결과창
    public TextMeshProUGUI resultCanvasText;      // 🔥 결과 텍스트

    private void Start()
    {
        UpdateButtonState();
        submitButton.onClick.AddListener(OnSubmit);

        // 목표덱 자동 연결
        if (goalDeckManager == null)
            goalDeckManager = FindObjectOfType<GoalDeckManager>();

        // 결과창은 시작 시 비활성화
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
            Debug.Log("❌ 제출 불가: 최소 2장 필요!");
            return;
        }

        Debug.Log("📤 제출 버튼 클릭");

        // --------------------------------
        // 1) 플레이어 카드 CardData 수집
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
        // 3) 덱 평가
        // --------------------------------
        string playerRank = DeckEvaluator.EvaluateDeck(playerDeck);
        string goalRank = DeckEvaluator.EvaluateDeck(goalDeck);

        int playerValue = DeckEvaluator.GetRankValue(playerRank);
        int goalValue = DeckEvaluator.GetRankValue(goalRank);

        bool isClear = playerValue >= goalValue;

        // --------------------------------
        // 4) 결과 출력 (새 결과창 Canvas)
        // --------------------------------
        if (resultCanvas != null)
        {
            resultCanvas.SetActive(true); // 🔥 결과창 활성화

            resultCanvasText.text =
                $"<size=50><b>RESULT</b></size>\n\n" +
                $"<color=white>Player : {playerRank}</color>\n" +
                $"<color=white>Goal : {goalRank}</color>\n\n" +
                (isClear
                    ? "<color=#FFD700><size=55><b>CLEAR!</b></size></color>"
                    : "<color=red><size=55><b>FAIL</b></size></color>");
        }

        Debug.Log($"🎮 RESULT → Player:{playerRank}  Goal:{goalRank}  Clear:{isClear}");
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

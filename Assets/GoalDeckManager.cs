using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GoalDeckManager : MonoBehaviour
{
    [Header("Goal Deck UI")]
    public RectTransform goalPanel;
    public GameObject cardPrefab;
    public CardManager cardManager;

    [Header("Description Text")]
    public TextMeshProUGUI descriptionText;   // 🔥 추가된 부분

    private List<Sprite> goalSprites = new List<Sprite>();
    public string StageGoalRank = "TwoPair";


    void Start()
    {
        CreateGoalTwoPair();
        ShowGoalDeck();
        ShowDescription();     // 🔥 추가
    }

    // --------------------------------------------
    // 🔥 목표 투페어 덱 구성하기
    // --------------------------------------------
    private void CreateGoalTwoPair()
    {
        goalSprites.Clear();

        goalSprites.Add(CardManager.GetCardSprite("S", 10));  // 10♠
        goalSprites.Add(CardManager.GetCardSprite("D", 10));  // 10♦
        goalSprites.Add(CardManager.GetCardSprite("C", 6));   // 6♣
        goalSprites.Add(CardManager.GetCardSprite("H", 6));   // 6♥
        goalSprites.Add(CardManager.GetCardSprite("S", 1));   // A♠ (킥커)
    }

    // --------------------------------------------
    // 🔥 화면 좌상단에 목표 덱 표시
    // --------------------------------------------
    private void ShowGoalDeck()
    {
        if (goalPanel == null || cardPrefab == null)
        {
            Debug.LogError("GoalDeckPanel 또는 CardPrefab이 연결되지 않음!");
            return;
        }

        foreach (Transform child in goalPanel)
        {
            if (child.name.Contains("GoalCard"))
                Destroy(child.gameObject);
        }

        float offsetX = 10f;

        foreach (var spr in goalSprites)
        {
            GameObject card = Instantiate(cardPrefab, goalPanel);
            card.name = "GoalCard";

            Image img = card.GetComponent<Image>();
            img.sprite = spr;

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            rt.sizeDelta = new Vector2(75f, 105f);
            rt.anchoredPosition = new Vector2(offsetX, -10f);

            offsetX += 55f;
        }
    }

    // --------------------------------------------
    // 🔥 목표족보 설명 텍스트 출력
    // --------------------------------------------
    private void ShowDescription()
    {
        if (descriptionText == null)
        {
            Debug.LogWarning("⚠ Goal Description Text 연결 안됨!");
            return;
        }

        descriptionText.text = "Stage 1: Two Pair";
    }

    public List<CardData> GetGoalDeckAsCardData()
    {
        List<CardData> list = new List<CardData>();

        foreach (Sprite spr in goalSprites)
        {
            CardData data = CardDatabase.GetCardDataFromSprite(spr);
            if (data != null)
                list.Add(data);
        }

        return list;
    }

    public string GetGoalRank()
    {
        return StageGoalRank;
    }
}

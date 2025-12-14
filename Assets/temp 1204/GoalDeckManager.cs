using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalDeckManager : MonoBehaviour
{
    [Header("Goal Deck UI")]
    public RectTransform goalPanel;
    public GameObject cardPrefab;
    public CardManager cardManager;

    [Header("Description Text")]
    public TextMeshProUGUI descriptionText;

    private readonly List<Sprite> goalSprites = new List<Sprite>();

    // 포커 족보 이름 (DeckEvaluator에서 사용하는 문자열)
    public string StageGoalRank = "TwoPair";

    private void Start()
    {
        // ✅ StageManager가 있으면 StageManager가 stageIndex에 맞춰 SetupGoalForStage를 호출하므로
        // 여기서 기본값(1스테이지)을 강제로 세팅하지 않는다.
        // (이게 있으면 시작 시 Stage1 UI가 먼저 찍혀서 튜토리얼 UI/텍스트 흐름이 꼬일 수 있음)
        if (StageManager.Instance == null)
        {
            SetupGoalForStage(1);
        }
    }

    // ============================================================
    // 🔥 스테이지별 목표 덱 설정
    //   0: 튜토리얼 (One Pair)
    //   1: Two Pair
    //   2: Three of a Kind
    //   3: Flush
    //   4: Full House
    // ============================================================
    public void SetupGoalForStage(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0:
                CreateGoalOnePair_Tutorial();
                StageGoalRank = "OnePair";
                break;

            case 1:
                CreateGoalTwoPair();
                StageGoalRank = "TwoPair";
                break;

            case 2:
                CreateGoalThreeOfAKind();
                StageGoalRank = "ThreeOfAKind";
                break;

            case 3:
                CreateGoalFlush();
                StageGoalRank = "Flush";
                break;

            case 4:
                CreateGoalFullHouse();
                StageGoalRank = "FullHouse";
                break;

            default:
                Debug.LogWarning($"Unknown stageIndex {stageIndex}, 기본값 FullHouse 사용");
                CreateGoalFullHouse();
                StageGoalRank = "FullHouse";
                break;
        }

        ShowGoalDeck();
        ShowDescription(stageIndex);
    }

    // --------------------------------------------
    // 0) 튜토리얼용 One Pair 예시 (단순 예시용)
    // --------------------------------------------
    private void CreateGoalOnePair_Tutorial()
    {
        goalSprites.Clear();

        // 예: 9♠ 9♥ 3♦ 6♣ K♠ (예시)
        goalSprites.Add(CardManager.GetCardSprite("S", 9));   // 9♠
        goalSprites.Add(CardManager.GetCardSprite("H", 9));   // 9♥
        goalSprites.Add(CardManager.GetCardSprite("D", 3));   // 3♦
        goalSprites.Add(CardManager.GetCardSprite("C", 6));   // 6♣
        goalSprites.Add(CardManager.GetCardSprite("S", 13));  // K♠
    }

    // --------------------------------------------
    // 1) 목표 투페어 덱 구성하기
    // --------------------------------------------
    private void CreateGoalTwoPair()
    {
        goalSprites.Clear();

        goalSprites.Add(CardManager.GetCardSprite("S", 10));  // 10♠
        goalSprites.Add(CardManager.GetCardSprite("D", 10));  // 10♦
        goalSprites.Add(CardManager.GetCardSprite("C", 6));   // 6♣
        goalSprites.Add(CardManager.GetCardSprite("H", 6));   // 6♥
        goalSprites.Add(CardManager.GetCardSprite("S", 1));   // A♠
    }

    // --------------------------------------------
    // 2) 목표 트리플(Three of a Kind)
    // --------------------------------------------
    private void CreateGoalThreeOfAKind()
    {
        goalSprites.Clear();

        goalSprites.Add(CardManager.GetCardSprite("S", 7));   // 7♠
        goalSprites.Add(CardManager.GetCardSprite("D", 7));   // 7♦
        goalSprites.Add(CardManager.GetCardSprite("H", 7));   // 7♥
        goalSprites.Add(CardManager.GetCardSprite("C", 13));  // K♣
        goalSprites.Add(CardManager.GetCardSprite("D", 3));   // 3♦
    }

    // --------------------------------------------
    // 3) 목표 플러쉬
    // --------------------------------------------
    private void CreateGoalFlush()
    {
        goalSprites.Clear();

        goalSprites.Add(CardManager.GetCardSprite("H", 2));   // 2♥
        goalSprites.Add(CardManager.GetCardSprite("H", 6));   // 6♥
        goalSprites.Add(CardManager.GetCardSprite("H", 9));   // 9♥
        goalSprites.Add(CardManager.GetCardSprite("H", 11));  // J♥
        goalSprites.Add(CardManager.GetCardSprite("H", 13));  // K♥
    }

    // --------------------------------------------
    // 4) 목표 풀하우스
    // --------------------------------------------
    private void CreateGoalFullHouse()
    {
        goalSprites.Clear();

        goalSprites.Add(CardManager.GetCardSprite("S", 12));  // Q♠
        goalSprites.Add(CardManager.GetCardSprite("D", 12));  // Q♦
        goalSprites.Add(CardManager.GetCardSprite("H", 12));  // Q♥
        goalSprites.Add(CardManager.GetCardSprite("C", 9));   // 9♣
        goalSprites.Add(CardManager.GetCardSprite("D", 9));   // 9♦
    }

    // --------------------------------------------
    // 목표 덱 표시
    // --------------------------------------------
    private void ShowGoalDeck()
    {
        if (goalPanel == null || cardPrefab == null)
        {
            Debug.LogError("GoalDeckPanel 또는 CardPrefab이 연결되지 않음!");
            return;
        }

        for (int i = goalPanel.childCount - 1; i >= 0; i--)
        {
            Transform child = goalPanel.GetChild(i);
            if (child != null && child.name.Contains("GoalCard"))
                Destroy(child.gameObject);
        }

        float offsetX = 10f;

        foreach (var spr in goalSprites)
        {
            GameObject card = Instantiate(cardPrefab, goalPanel);
            card.name = "GoalCard";

            Image img = card.GetComponent<Image>();
            if (img != null) img.sprite = spr;

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
    // 목표족보 설명 텍스트 출력
    // --------------------------------------------
    private void ShowDescription(int stageIndex)
    {
        if (descriptionText == null)
        {
            Debug.LogWarning("⚠ Goal Description Text 연결 안됨!");
            return;
        }

        string rankName = StageGoalRank switch
        {
            "OnePair" => "One Pair",
            "TwoPair" => "Two Pair",
            "ThreeOfAKind" => "Three of a Kind",
            "Flush" => "Flush",
            "FullHouse" => "Full House",
            _ => StageGoalRank
        };

        if (stageIndex == 0)
            descriptionText.text = $"Tutorial: {rankName}";
        else
            descriptionText.text = $"Stage {stageIndex}: {rankName}";
    }

    // ============================================================
    public List<CardData> GetGoalDeckAsCardData()
    {
        List<CardData> list = new List<CardData>();

        foreach (Sprite spr in goalSprites)
        {
            CardData data = CardDatabase.GetCardDataFromSprite(spr);
            if (data != null) list.Add(data);
        }

        return list;
    }

    public List<Sprite> GetGoalSprites() => goalSprites;

    public string GetGoalRank() => StageGoalRank;
}

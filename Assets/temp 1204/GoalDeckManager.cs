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

    private List<Sprite> goalSprites = new List<Sprite>();

    // 포커 족보 이름 (DeckEvaluator에서 사용하는 문자열)
    public string StageGoalRank = "TwoPair";

    void Start()
    {
        // StageManager를 아직 안 쓴다면 기본적으로 1스테이지 세팅
        // StageManager가 있다면, Start 이후에 SetupGoalForStage(currentStage)를 다시 호출해도 됨.
        SetupGoalForStage(1);
    }

    // ============================================================
    // 🔥 스테이지별 목표 덱 설정
    //   1: Two Pair
    //   2: Three of a Kind
    //   3: Flush
    //   4: Full House
    // ============================================================
    public void SetupGoalForStage(int stageIndex)
    {
        switch (stageIndex)
        {
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
                // 범위를 벗어나면 마지막 스테이지 규칙 유지
                Debug.LogWarning($"Unknown stageIndex {stageIndex}, 기본값 FullHouse 사용");
                CreateGoalFullHouse();
                StageGoalRank = "FullHouse";
                break;
        }

        ShowGoalDeck();
        ShowDescription(stageIndex);
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
        goalSprites.Add(CardManager.GetCardSprite("S", 1));   // A♠ (킥커)
    }

    // --------------------------------------------
    // 2) 목표 트리플(Three of a Kind)
    // --------------------------------------------
    private void CreateGoalThreeOfAKind()
    {
        goalSprites.Clear();

        // 예: 7♠ 7♦ 7♥  K♣  3♦
        goalSprites.Add(CardManager.GetCardSprite("S", 7));   // 7♠
        goalSprites.Add(CardManager.GetCardSprite("D", 7));   // 7♦
        goalSprites.Add(CardManager.GetCardSprite("H", 7));   // 7♥
        goalSprites.Add(CardManager.GetCardSprite("C", 13));  // K♣
        goalSprites.Add(CardManager.GetCardSprite("D", 3));   // 3♦
    }

    // --------------------------------------------
    // 3) 목표 플러쉬 (같은 문양 5장, 스트레이트는 아님)
    // --------------------------------------------
    private void CreateGoalFlush()
    {
        goalSprites.Clear();

        // 예: 하트 플러쉬 2♥ 6♥ 9♥ J♥ K♥ (연속되지 않게)
        goalSprites.Add(CardManager.GetCardSprite("H", 2));   // 2♥
        goalSprites.Add(CardManager.GetCardSprite("H", 6));   // 6♥
        goalSprites.Add(CardManager.GetCardSprite("H", 9));   // 9♥
        goalSprites.Add(CardManager.GetCardSprite("H", 11));  // J♥
        goalSprites.Add(CardManager.GetCardSprite("H", 13));  // K♥
    }

    // --------------------------------------------
    // 4) 목표 풀하우스 (AAA + BB)
    // --------------------------------------------
    private void CreateGoalFullHouse()
    {
        goalSprites.Clear();

        // 예: Q♠ Q♦ Q♥  9♣ 9♦
        goalSprites.Add(CardManager.GetCardSprite("S", 12));  // Q♠
        goalSprites.Add(CardManager.GetCardSprite("D", 12));  // Q♦
        goalSprites.Add(CardManager.GetCardSprite("H", 12));  // Q♥
        goalSprites.Add(CardManager.GetCardSprite("C", 9));   // 9♣
        goalSprites.Add(CardManager.GetCardSprite("D", 9));   // 9♦
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
    private void ShowDescription(int stageIndex)
    {
        if (descriptionText == null)
        {
            Debug.LogWarning("⚠ Goal Description Text 연결 안됨!");
            return;
        }

        string rankName = StageGoalRank switch
        {
            "TwoPair" => "Two Pair",
            "ThreeOfAKind" => "Three of a Kind",
            "Flush" => "Flush",
            "FullHouse" => "Full House",
            _ => StageGoalRank
        };

        descriptionText.text = $"Stage {stageIndex}: {rankName}";
    }

    // ============================================================
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

    public List<Sprite> GetGoalSprites()
    {
        return goalSprites;
    }

    public string GetGoalRank()
    {
        return StageGoalRank;
    }
}

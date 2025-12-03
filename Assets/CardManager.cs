using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    public Sprite[] cardSprites; // Inspector에서 받는 전체 스프라이트 (백 포함 가능)
    private static CardManager instance;

    private List<Sprite> frontSprites = new List<Sprite>(); // 🔥 앞면만 모은 리스트
    private List<int> deck = new List<int>();

    void Awake()
    {
        // 싱글톤
        if (instance == null)
            instance = this;
        else { Destroy(gameObject); return; }

        // 🔥 앞면 스프라이트만 자동 필터
        frontSprites = cardSprites
            .Where(spr => IsFrontCard(spr.name))
            .ToList();

        if (frontSprites.Count != 52)
            Debug.LogWarning($"⚠ 앞면 카드 수가 이상합니다: {frontSprites.Count}장");

        // 덱 초기화
        deck.Clear();
        for (int i = 0; i < frontSprites.Count; i++)
            deck.Add(i);
    }

    // 🔥 카드 이름이 앞면인지 체크하는 규칙 함수
    private bool IsFrontCard(string name)
    {
        // 예: AH, 10D, QS, 3C 형태만 인정
        if (name.Length < 2 || name.Length > 3)
            return false;

        string rankPart = name.Length == 3 ? name.Substring(0, 2) : name.Substring(0, 1);
        string suitPart = name.Substring(name.Length - 1, 1);

        bool validRank =
            rankPart == "A" ||
            rankPart == "J" ||
            rankPart == "Q" ||
            rankPart == "K" ||
            int.TryParse(rankPart, out _);

        bool validSuit = suitPart == "S" || suitPart == "H" || suitPart == "D" || suitPart == "C";

        return validRank && validSuit;
    }

    // 🔥 이름으로 스프라이트 찾기
    public static Sprite GetCardSprite(string suit, int rank)
    {
        string rankStr = rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => rank.ToString()
        };

        string key = $"{rankStr}{suit}";

        foreach (var spr in instance.frontSprites)
        {
            if (spr.name == key)
                return spr;
        }

        Debug.LogWarning($"❌ 스프라이트 없음: {key}");
        return null;
    }

    // 🔥 덱에서 랜덤 카드 N장 뽑기
    public List<Sprite> DrawRandomCards(int count)
    {
        ShuffleDeck();

        List<Sprite> result = new List<Sprite>();
        for (int i = 0; i < count; i++)
            result.Add(frontSprites[deck[i]]);

        return result;
    }

    // 🔥 셔플
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int r = Random.Range(i, deck.Count);
            (deck[i], deck[r]) = (deck[r], deck[i]);
        }
    }
}

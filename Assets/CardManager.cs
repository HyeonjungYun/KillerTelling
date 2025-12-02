using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public Sprite[] cardSprites; // 52장 Sprite 전체
    private static CardManager instance;

    private List<int> deck = new List<int>(); // 0~51 번호 덱

    void Awake()
    {
        // 싱글톤 처리
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 덱 초기화
        deck.Clear();
        for (int i = 0; i < cardSprites.Length; i++)
            deck.Add(i);
    }

    // 🔥 ① 조원이 만든 기능: 이름으로 카드 스프라이트 찾기
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

        string key = $"{rankStr}{suit}"; // 예: “QH"

        foreach (var sprite in instance.cardSprites)
        {
            if (sprite != null && sprite.name == key)
                return sprite;
        }

        Debug.LogWarning($"❌ 스프라이트 없음: {key}");
        return null;
    }

    // 🔥 ② 네가 만든 기능: 52장 덱에서 랜덤 카드 n장 뽑기
    public List<Sprite> DrawRandomCards(int count)
    {
        ShuffleDeck();

        List<Sprite> result = new List<Sprite>();
        for (int i = 0; i < count; i++)
            result.Add(cardSprites[deck[i]]);

        return result;
    }

    // 🔥 ③ 덱 셔플
    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int r = Random.Range(i, deck.Count);
            (deck[i], deck[r]) = (deck[r], deck[i]);
        }
    }

    public static CardData GetCardDataBySprite(Sprite spr)
    {
        if (spr == null || string.IsNullOrEmpty(spr.name))
        {
            Debug.LogError("❌ Sprite null or invalid");
            return null;
        }

        string name = spr.name;   // 예: "10S", "QH", "AD", "7C"

        // 마지막 글자가 suit
        string suit = name.Substring(name.Length - 1, 1);   // "S","H","D","C"

        // 나머지가 rank 문자
        string rankStr = name.Substring(0, name.Length - 1); // "10", "Q", "A"…

        int rank = rankStr switch
        {
            "A" => 1,
            "J" => 11,
            "Q" => 12,
            "K" => 13,
            _ => int.TryParse(rankStr, out int n) ? n : -1
        };

        if (rank == -1)
        {
            Debug.LogError($"❌ Rank parsing failed: {spr.name}");
            return null;
        }

        return new CardData(suit, rank, spr);
    }

}



/*using UnityEngine;
using System.Collections.Generic;

public class CardManager : MonoBehaviour
{
    public Sprite[] cardSprites; // 52장 Sprite 전체
    private static CardManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ============================================================
    // 🔥 1) SUIT + RANK → SPRITE 찾기
    // ============================================================
    public static Sprite GetSprite(string suit, int rank)
    {
        if (instance == null)
        {
            Debug.LogError("❌ CardManager instance not found");
            return null;
        }

        string rankStr = rank switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => rank.ToString()
        };

        string key = $"{rankStr}{suit}";

        foreach (var spr in instance.cardSprites)
            if (spr != null && spr.name == key)
                return spr;

        Debug.LogWarning($"❌ Sprite not found: {key}");
        return null;
    }

    // ============================================================
    // 🔥 2) SUIT + RANK → CardData (정식)
    // ============================================================
    public static CardData CreateCardData(string suit, int rank)
    {
        Sprite spr = GetSprite(suit, rank);
        if (spr == null)
            return null;

        return new CardData(suit, rank, spr);
    }

    // ============================================================
    // 🔥 3) Sprite → CardData 변환 (중복 제거한 최종 버전)
    // ============================================================
    public static CardData ParseCardData(Sprite spr)
    {
        if (spr == null || string.IsNullOrEmpty(spr.name))
            return null;

        string name = spr.name;
        string suit = name.Substring(name.Length - 1, 1);
        string rankStr = name.Substring(0, name.Length - 1);

        int rank = rankStr switch
        {
            "A" => 1,
            "J" => 11,
            "Q" => 12,
            "K" => 13,
            _ => int.TryParse(rankStr, out int n) ? n : -1
        };

        if (rank == -1)
            return null;

        return new CardData(suit, rank, spr);
    }

    // ============================================================
    // 🔥 4) 랜덤 N장 뽑기 (필요하면 유지)
    // ============================================================
    public List<Sprite> DrawRandomSprites(int count)
    {
        List<Sprite> list = new List<Sprite>(cardSprites);
        Shuffle(list);

        return list.GetRange(0, count);
    }

    private void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
*/
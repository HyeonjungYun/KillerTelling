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
}

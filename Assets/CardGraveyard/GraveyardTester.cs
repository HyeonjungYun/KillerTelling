using System.Collections.Generic;
using UnityEngine;

public class GraveyardTester : MonoBehaviour
{
    public CardGraveyardManager graveyard;

    void Start()
    {
        List<Sprite> testCards = new List<Sprite>();

        // 테스트용 카드 임시 입력
        // 이 부분은 네 프로젝트 환경에 맞게 Sprite 로딩 가능
        // testCards.Add(someSprite);

        graveyard.AddCards(testCards);   // 🔥 함수 이름 최신 버전으로 변경됨

        Dictionary<string, int> suitCounts = GetSuitCountsFromManager();
        foreach (var kv in suitCounts)
        {
            Debug.Log($"{kv.Key} : {kv.Value}");
        }
    }

    // 🔥 최신 구조에서는 GetSuitCounts 없음 → 직접 함수 구현
    private Dictionary<string, int> GetSuitCountsFromManager()
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        foreach (var sprite in graveyard.StoredSprites)   // StoredSprites 프로퍼티 추가 필요
        {
            // 카드 sprite.name 이 "Spade_7" 이런 식이라면 suit 추출 필요
            string suit = ExtractSuit(sprite.name);

            if (!result.ContainsKey(suit))
                result[suit] = 0;

            result[suit]++;
        }

        return result;
    }

    private string ExtractSuit(string name)
    {
        // "Hearts_10" → "Hearts"
        if (name.Contains("_"))
            return name.Split('_')[0];

        return "Unknown";
    }
}

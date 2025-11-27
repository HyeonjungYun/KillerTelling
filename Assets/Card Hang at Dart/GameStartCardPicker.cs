using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameStartCardPicker : MonoBehaviour
{
    public WallCardPlacer wallPlacer;

    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private System.Collections.IEnumerator DelayedInit()
    {
        // DeckManager가 덱 UI를 생성할 시간을 준다
        yield return new WaitForSeconds(0.1f);

        // 전체 DeckCard 가져오기
        DeckCard[] allCards = FindObjectsOfType<DeckCard>();

        List<DeckCard> candidates = allCards
            .Where(c => c.CardSprite != null)
            .ToList();

        if (candidates.Count < 5)
        {
            Debug.LogError("❌ 덱 패널에 카드가 5장 미만입니다!");
            yield break;
        }

        // 5장 랜덤 선택
        List<DeckCard> selected = new List<DeckCard>();
        for (int i = 0; i < 5; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            selected.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        // 패널에서 비활성화 + 회색 처리
        foreach (var card in selected)
            card.MarkAsUsed();

        // 벽에 붙일 Sprite 리스트
        List<Sprite> sprites = selected.Select(c => c.CardSprite).ToList();

        // 벽에 카드 배치
        wallPlacer.PlaceCards(sprites);

        Debug.Log("🎯 [GameStartCardPicker] 5장 랜덤 선택 + 벽 부착 완료");
    }
}

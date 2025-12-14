using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameStartCardPicker : MonoBehaviour
{
    public WallCardPlacer wallPlacer;

    private void Start()
    {
        // StageManager 없는 옛 버전 대비
        if (StageManager.Instance == null)
        {
            SetupForStage(1);
        }
    }

    /// <summary>
    /// StageManager가 스테이지 전환/시작 시 호출.
    /// </summary>
    public void SetupForStage(int stageIndex)
    {
        StopAllCoroutines();

        if (stageIndex == 0)
        {
            // 튜토리얼: 고정 5장
            StartCoroutine(SetupTutorialCards());
        }
        else
        {
            // 일반 스테이지: 랜덤 5장
            StartCoroutine(DelayedInit(stageIndex));
        }
    }

    /// <summary>
    /// ✅ StageManager에서 호출하는 "과녁 카드만 정리"용 함수
    /// </summary>
    public void ClearTargetCards()
    {
        if (wallPlacer != null)
            wallPlacer.ClearTargetAreaOnly();
    }

    // ------------------------------------------------
    // 튜토리얼 : 과녁 고정 카드
    // 4하트, A클로버, 8다이아, K클로버, 7스페이드
    // ------------------------------------------------
    private IEnumerator SetupTutorialCards()
    {
        yield return new WaitForSeconds(0.1f);

        if (wallPlacer == null)
        {
            Debug.LogError("[GameStartCardPicker] WallCardPlacer 참조가 없습니다 (튜토리얼)");
            yield break;
        }

        // ✅ 1) 과녁 정리
        wallPlacer.ClearTargetAreaOnly();

        // ✅ 2) 고정 스프라이트 5장
        List<Sprite> sprites = new List<Sprite>
        {
            CardManager.GetCardSprite("H", 4),   // 4♥
            CardManager.GetCardSprite("C", 1),   // A♣
            CardManager.GetCardSprite("D", 8),   // 8♦
            CardManager.GetCardSprite("C", 13),  // K♣
            CardManager.GetCardSprite("S", 7),   // 7♠
        };

        // ✅ 3) 덱에서 동일 카드들 "사용 처리" (중복 등장/후보 꼬임 방지)
        MarkTheseSpritesAsUsedInDeck(sprites);

        // ✅ 4) 과녁 배치
        wallPlacer.PlaceCards(sprites);

        Debug.Log("🎯 [Tutorial] 고정 과녁 카드 배치 완료 (4H, AC, 8D, KC, 7S)");
    }

    // ------------------------------------------------
    // 일반 스테이지용 랜덤 선정
    // ------------------------------------------------
    private IEnumerator DelayedInit(int stageIndex)
    {
        yield return new WaitForSeconds(0.1f);

        if (wallPlacer == null)
        {
            Debug.LogError("[GameStartCardPicker] WallCardPlacer 참조가 없습니다 (일반)");
            yield break;
        }

        // ✅ 1) 과녁 정리
        wallPlacer.ClearTargetAreaOnly();

        // ✅ 2) 사용 가능한 덱 카드 후보 수집
        DeckCard[] allCards = FindObjectsOfType<DeckCard>();
        List<DeckCard> candidates = allCards
            .Where(c => c.CardSprite != null
                        && c.TryGetComponent(out Image img)
                        && img.raycastTarget)
            .ToList();

        if (candidates.Count < 5)
        {
            Debug.LogError($"❌ [GameStartCardPicker] 스테이지 {stageIndex} 에서 덱 카드가 5장 미만입니다!");
            yield break;
        }

        // ✅ 3) 5장 랜덤 선택
        List<DeckCard> selected = new List<DeckCard>();
        for (int i = 0; i < 5; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            selected.Add(candidates[idx]);
            candidates.RemoveAt(idx);
        }

        // ✅ 4) 덱에서 사용 처리
        foreach (var card in selected)
            card.MarkAsUsed();

        // ✅ 5) 과녁 배치
        List<Sprite> sprites = selected.Select(c => c.CardSprite).ToList();
        wallPlacer.PlaceCards(sprites);

        Debug.Log($"🎯 [GameStartCardPicker] Stage {stageIndex} → 5장 랜덤 선택 + 벽 부착 완료");
    }

    // ------------------------------------------------
    // 덱에서 특정 스프라이트들을 찾아 "사용 처리"
    // ------------------------------------------------
    private void MarkTheseSpritesAsUsedInDeck(List<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0) return;

        DeckCard[] all = FindObjectsOfType<DeckCard>();

        foreach (var spr in sprites)
        {
            if (spr == null) continue;

            // 덱에 동일 sprite를 가진 DeckCard를 찾아서 사용 처리
            var dc = all.FirstOrDefault(x => x != null && x.CardSprite == spr);
            if (dc != null)
                dc.MarkAsUsed();
            else
                Debug.LogWarning($"⚠ [Tutorial] 덱에서 해당 스프라이트를 찾지 못함: {spr.name}");
        }
    }
}
